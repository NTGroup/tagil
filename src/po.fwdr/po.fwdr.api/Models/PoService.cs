using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using po.fwdr.api.AppConfigurators;
using po.fwdr.contract.Companies;
using po.fwdr.contract.Locations;
using po.fwdr.contract.Markups;
using po.fwdr.contract.Orders.Output;
using po.fwdr.contract.Planes;
using po.fwdr.contract.Tenants;

namespace po.fwdr.api.Models
{
	public class PoService
	{
		const string PoConnectionStringKey = "PoDb";
		const int MaxLocationQuerySize = 5;

		public PoService()
		{
			_connectionString = ConfigurationManager.ConnectionStrings[PoConnectionStringKey].ConnectionString;
		}

		internal Task<LocationContract[]> FindLocationsAsync(IEnumerable<string> locations)
		{
			bool askForAll = locations == null;
			List<LocationContract> res = new List<LocationContract>();
			string[] trgLocs = null;
			if (!askForAll)
			{
				trgLocs = locations
					.Where(l => !string.IsNullOrEmpty(l) && l.Length <= MaxLocationQuerySize)
					.Select(l => l.ToUpperInvariant())
					.ToArray();
			}

			if (!askForAll && trgLocs.Length == 0)
				return Task.FromResult(GetEmptyLocations());

			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand cmd = new OracleCommand("ntg.geo_api.get_utc_offset", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);
				if (!askForAll)
				{
					OracleParameter pIn = new OracleParameter();
					pIn.ParameterName = "P_IATA";
					pIn.OracleDbType = OracleDbType.Varchar2;
					pIn.Direction = ParameterDirection.Input;
					pIn.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
					pIn.Value = trgLocs;
					pIn.Size = trgLocs.Length;
					cmd.Parameters.Add(pIn);
				}

				connection.Open();
				OracleDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					res.Add(new LocationContract
					{
						Code = reader.GetString(0),
						UtcOffset = reader.GetFloat(1)
					});
				}

				reader.Close();
			}

			return Task.FromResult(res.ToArray());
		}

		internal Task<MarkupBundleContract> FindMarkupsAsync()
		{
			List<PerPassengerMarkupContract> pasMus = new List<PerPassengerMarkupContract>();
			List<PerSegmentMarkupContract> segMus = new List<PerSegmentMarkupContract>();
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ntg.markup_api.get_full";
				command.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);

				connection.Open();

				OracleDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{
					bool isSegment = reader.GetString(2) == "Y";
					string validatingCarrier = reader.GetString(0);
					string cos = reader.GetString(1);

					if (isSegment)
					{
						decimal fixValue = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
						int minLimit = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
						int maxLimit = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
						segMus.Add(
							new PerSegmentMarkupContract(
								validatingCarrier,
								cos,
								fixValue,
								minLimit,
								maxLimit
							)
						);
						continue;
					}

					bool isRated = !reader.IsDBNull(6);
					pasMus.Add(
						new PerPassengerMarkupContract(
							validatingCarrier,
							cos,
							isRated ? reader.GetDecimal(7) : reader.GetDecimal(5),
							isRated ? (reader.GetDecimal(6) / 100M) : 0
						)
					);
				}

				reader.Close();
				connection.Close();
			}

			MarkupBundleContract res = new MarkupBundleContract
			{
				PerPassenger = pasMus.ToArray(),
				PerSegments = segMus.ToArray()
			};

			return Task.FromResult(res);
		}

		internal async Task<int> RegisterAsync(
			string tenantId,
			string nqtId,
			string nqtObject,
			string pnrState,
			DateTime? systemTimeLimit,
			decimal? totalAmount,
			decimal? totalMarkup,
			string pnrId
		)
		{
			int i;
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ord.fwdr.avia_register";
				command.BindByName = true;

				command.Parameters.Add("p_nqt_id", nqtId);
				command.Parameters.Add("p_tenant_id", tenantId);
				command.Parameters.Add("p_pnr_object", nqtObject);
				command.Parameters.Add("p_time_limit", systemTimeLimit);
				command.Parameters.Add("p_total_amount", totalAmount);
				command.Parameters.Add("p_total_markup", totalMarkup);
				command.Parameters.Add("p_pnr_id", pnrId);
				command.Parameters.Add("p_nqt_status", pnrState);

				connection.Open();
				i = await command.ExecuteNonQueryAsync()
					.ConfigureAwait(false);
			}
			return i;
		}

		internal async Task<int> PayAsync(string nqtId)
		{
			int i;
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ord.fwdr.avia_pay";
				command.BindByName = true;

				command.Parameters.Add("p_nqt_id", nqtId);

				connection.Open();
				i = await command.ExecuteNonQueryAsync()
					.ConfigureAwait(false);
			}
			return i;
		}

		internal async Task<int> ManualAsync(string nqtId)
		{
			int i;
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ord.fwdr.avia_manual";
				command.BindByName = true;

				command.Parameters.Add("p_nqt_id", nqtId);

				connection.Open();
				i = await command.ExecuteNonQueryAsync()
					.ConfigureAwait(false);
			}
			return i;
		}

		internal async Task<List<GeoItemContract>> ListGeoItemsAsync()
		{
			List<GeoItemContract> res = new List<GeoItemContract>();
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ntg.geo_api.geo_get_list";
				command.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);

				connection.Open();

				OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync()
					.ConfigureAwait(false);

				while (reader.Read())
				{
					res.Add(
						new GeoItemContract
						{
							Code = reader["IATA"].ToString(),
							NameEn = reader["NAME"].ToString(),
							NameRu = reader["NLS_NAME"].ToString(),
							CityCode = reader["CITY_IATA"].ToString(),
							CityNameEn = reader["CITY_NAME"].ToString(),
							CityNameRu = reader["CITY_NLS_NAME"].ToString()
						}
					);
				}

				reader.Close();
				connection.Close();
			}

			return res;
		}

		internal async Task<List<CompanyContract>> ListCompaniesAsync()
		{
			List<CompanyContract> res = new List<CompanyContract>();
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ntg.geo_api.airline_get_list";
				command.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);

				connection.Open();

				OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync()
					.ConfigureAwait(false);

				while (reader.Read())
				{
					res.Add(
						new CompanyContract
						{
							Code = reader["IATA"].ToString(),
							NameEn = reader["NAME"].ToString(),
							NameRu = reader["NLS_NAME"].ToString()
						}
					);
				}

				reader.Close();
				connection.Close();
			}

			return res;
		}

		internal async Task<List<PlaneTypeContract>> ListPlaneTypesAsync()
		{
			List<PlaneTypeContract> res = new List<PlaneTypeContract>();
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ntg.geo_api.airplane_get_list";
				command.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);

				connection.Open();

				OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync()
					.ConfigureAwait(false);

				while (reader.Read())
				{
					res.Add(
						new PlaneTypeContract
						{
							Code = reader["IATA"].ToString(),
							NameEn = reader["NAME"].ToString(),
							NameRu = reader["NLS_NAME"].ToString()
						}
					);
				}

				reader.Close();
				connection.Close();
			}

			return res;
		}

		internal async Task<List<PnrStatusItemContract>> ListPnrStatuses(string[] ids)
		{
			List<PnrStatusItemContract> res = new List<PnrStatusItemContract>();
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ord.fwdr.pnr_list";
				command.Parameters.Add(
					"ReturnValue",
					OracleDbType.RefCursor,
					ParameterDirection.ReturnValue
				);

				command.BindByName = true;

				string statusJsonList = JsonConvert.SerializeObject(
					ids.Select(s => new { id = s }).ToArray(),
					JsonContentConfig.GeneralSettings
				);

				command.Parameters.Add("p_nqt_id_list", statusJsonList);

				connection.Open();

				OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync()
					.ConfigureAwait(false);

				while (reader.Read())
				{
					res.Add(
						new PnrStatusItemContract
						{
							NqtStatus = reader["nqt_status"].ToString(),
							PnrId = reader["nqt_id"].ToString(),
							PoStatus = reader["po_status"].ToString(),
							PoMessage = reader["po_msg"].ToString()
						}
					);
				}

				reader.Close();
				connection.Close();
			}

			return res;
		}

		internal async Task<CommissionContract> GetCommission(string id)
		{
			decimal? fixValue = null;
			decimal? prctValue = null;
			int i = 0;
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "ord.fwdr.commission_get";
				command.BindByName = true;
				command.Parameters.Add("p_nqt_id", id);

				OracleParameter fixParam = command.Parameters.Add("o_fix", OracleDbType.Decimal, ParameterDirection.Output);
				OracleParameter prctParam = command.Parameters.Add("o_percent", OracleDbType.Decimal, ParameterDirection.Output);

				connection.Open();

				i = await command.ExecuteNonQueryAsync()
					.ConfigureAwait(false);


				if (!((OracleDecimal)prctParam.Value).IsNull)
					prctValue = ((OracleDecimal)prctParam.Value).Value;

				if (!((OracleDecimal)fixParam.Value).IsNull)
					fixValue = ((OracleDecimal)fixParam.Value).Value;

				connection.Close();
			}

			return new CommissionContract
			{
				FixValue = fixValue,
				PercentValue = prctValue
			};
		}

		internal async Task<TenantContract> GetTenant(string email)
		{
			int? res = null;
			using (OracleConnection connection = new OracleConnection(_connectionString))
			{
				OracleCommand command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "blng.fwdr.get_tenant";
				command.BindByName = true;
				command.Parameters.Add("p_email", email);
				OracleParameter retParam = command.Parameters.Add(
					"ReturnValue",
					OracleDbType.Int32,
					ParameterDirection.ReturnValue
				);

				connection.Open();

				await command.ExecuteScalarAsync();

				if (!((OracleDecimal)retParam.Value).IsNull)
					res = (int?)((OracleDecimal)retParam.Value).Value;

				connection.Close();
			}

			return new TenantContract
			{
				Email = email,
				TenantId = res.HasValue ? res.Value.ToString() : null
			};
		}

		private readonly string _connectionString;

		private static LocationContract[] GetEmptyLocations()
		{
			return Enumerable.Empty<LocationContract>().ToArray();
		}
	}
}
