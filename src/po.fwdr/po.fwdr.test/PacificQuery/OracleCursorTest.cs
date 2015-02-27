using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace po.fwdr.test.PacificQuery
{
	[TestFixture]
	public class OracleCursorTest
	{
		const string PoConnectionStringKey = "PoDb";

		[Test, Ignore("No real connection string")]
		public void CursorTest()
		{
			string cnnStr = ConfigurationManager.ConnectionStrings[PoConnectionStringKey].ConnectionString;

			using (OracleConnection con = new OracleConnection(cnnStr))
			{
				OracleCommand cmd = new OracleCommand("ntg.markup_api.get_airlines", con);
				cmd.CommandType = CommandType.StoredProcedure;

				// create parameter object for the cursor
				OracleParameter refCursor = new OracleParameter();

				// this is vital to set when using ref cursors
				refCursor.OracleDbType = OracleDbType.RefCursor;

				// this is a function return value so we must indicate that fact
				refCursor.Direction = ParameterDirection.ReturnValue;

				// add the parameter to the collection
				cmd.Parameters.Add(refCursor);

				OracleParameter p1 = new OracleParameter();
				p1.DbType = DbType.String;
				p1.ParameterName = "P_GDS";
				p1.Value = DBNull.Value;

				cmd.Parameters.Add(p1);

				OracleParameter p2 = new OracleParameter();
				p2.DbType = DbType.String;
				p2.ParameterName = "P_POS";
				p2.Value = DBNull.Value;

				con.Open();
				OracleDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					Console.WriteLine("{0}, {1}, {2}, {3}", reader.GetValue(0), reader.GetValue(1), reader.GetValue(2), reader.GetValue(3));
				}

				reader.Close();
			}
		}
	}
}
