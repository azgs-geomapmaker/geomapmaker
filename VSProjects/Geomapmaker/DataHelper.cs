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

		public delegate void UserLoginDelegate();
		public static event UserLoginDelegate UserLoginHandler;

		public static string connectionString;

		public static List<FeatureLayer> currentLayers = new List<FeatureLayer>();
		public static List<StandaloneTable> currentTables = new List<StandaloneTable>();


		public static void UserLogin(int uID, String uName) {
			userID = uID;
			userName = uName;
			UserLoginHandler?.Invoke();
		}
	}
}
