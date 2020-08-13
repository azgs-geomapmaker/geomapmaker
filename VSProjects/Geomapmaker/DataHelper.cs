using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker {
	public class DataHelper {
		public static int userID;
		public static String userName;

		public static string connectionString;
		public static ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties;

		public static List<FeatureLayer> currentLayers = new List<FeatureLayer>();
		public static List<StandaloneTable> currentTables = new List<StandaloneTable>();


		public delegate void UserLoginDelegate();
		public static event UserLoginDelegate UserLoginHandler;
		public static void UserLogin(int uID, String uName) {
			userID = uID;
			userName = uName;
			UserLoginHandler?.Invoke();
		}

		public delegate void ProjectSelectedDelegate();
		public static event ProjectSelectedDelegate ProjectSelectedHandler;
		public static void ProjectSelected() {
			ProjectSelectedHandler?.Invoke();
		}

	}
}
