using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;

namespace TaskConqueror
{
    public partial class App : Application
    {
        MainWindowViewModel _viewModel;

        public App()
            : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        
        static App()
        {
            // This code is used to test the app when using other cultures.
            //
            //System.Threading.Thread.CurrentThread.CurrentCulture =
            //    System.Threading.Thread.CurrentThread.CurrentUICulture =
            //        new System.Globalization.CultureInfo("it-IT");


            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            //
            FrameworkElement.LanguageProperty.OverrideMetadata(
              typeof(FrameworkElement),
              new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string dbDir = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "TaskConqueror");
            AppDomain.CurrentDomain.SetData("DataDirectory", dbDir);

            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(ConfigurationManager.ConnectionStrings["TaskConquerorSql"].ConnectionString))
                {
                    CreateDatabase(dbDir);

                    connection.Open();
                    UpdateDatabaseSchema(GetDatabaseSchemaVersion(connection), connection);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                WPFMessageBox.Show(TaskConqueror.Properties.Resources.Error_Encountered, "Error encountered while preparing database: " + ex.Message, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            MainWindow window = new MainWindow();

            _viewModel = new MainWindowViewModel();

            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                _viewModel.RequestClose -= handler;
                window.Close();
            };
            _viewModel.RequestClose += handler;

            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            window.DataContext = _viewModel;

            window.Show();
        }

        private int GetDatabaseSchemaVersion(SqlCeConnection connection)
        {
            int version = 0;

            bool settingsTableExists = false;
            
            string checkTable = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Setting'";

            SqlCeCommand command = new SqlCeCommand(checkTable, connection);
            SqlCeDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                settingsTableExists = true;
            }

            //  check if settings table exists
            if (settingsTableExists)
            {
                command = new SqlCeCommand("SELECT Value FROM Setting WHERE (Name = N'DBSchemaVersion')", connection);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    version = Int32.Parse(reader["Value"].ToString());
                }

                reader.Close();
            }
                        
            return version;
        }

        private void UpdateDatabaseSchema(int version, SqlCeConnection connection)
        {
            // if version is zero
            if (version == 0)
            {
                // run script to insert setting table
                SqlCeCommand createCommand = connection.CreateCommand();
                createCommand.CommandText =
                  "CREATE TABLE Setting (" +
                  "  Name nvarchar(100) CONSTRAINT pkName PRIMARY KEY," +
                  "  Value nvarchar(100)" +
                  ")";
                createCommand.ExecuteNonQuery();

                SqlCeCommand insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                  "INSERT Setting (" +
                  "  Name, Value)" +
                  "  VALUES ('DBSchemaVersion', '1')";
                insertCommand.ExecuteNonQuery();
            }
            else if (version == 1)
            {
                // run script to insert sort order column
                SqlCeCommand createCommand = connection.CreateCommand();
                createCommand.CommandText =
                  "ALTER TABLE Task ADD SortOrder INTEGER DEFAULT NULL";
                createCommand.ExecuteNonQuery();

                // run script to populate sort order values
                int sortOrder = 1;
                SqlCeCommand selectTasksCommand = new SqlCeCommand("SELECT TaskID FROM Task WHERE (IsActive = 1) ORDER BY Title", connection);
                SqlCeDataReader tasksReader = selectTasksCommand.ExecuteReader();
                while (tasksReader.Read())
                {
                    int taskId = Int32.Parse(tasksReader["TaskID"].ToString());

                    SqlCeCommand sortUpdateCommand = connection.CreateCommand();
                    sortUpdateCommand.CommandText =
                      "UPDATE Task SET SortOrder=@sortOrder WHERE TaskID=@taskId";

                    SqlCeParameter sortOrderParam = new SqlCeParameter("@sortOrder", SqlDbType.Int);
                    sortOrderParam.Value = sortOrder;
                    sortUpdateCommand.Parameters.Add(sortOrderParam);

                    SqlCeParameter taskIdParam = new SqlCeParameter("@taskId", SqlDbType.Int);
                    taskIdParam.Value = taskId;
                    sortUpdateCommand.Parameters.Add(taskIdParam);
                    
                    sortUpdateCommand.Prepare();
                    sortUpdateCommand.ExecuteNonQuery();
                    sortOrder++;
                }

                tasksReader.Close();

                // run script to update schema version setting
                SqlCeCommand updateCommand = connection.CreateCommand();
                updateCommand.CommandText =
                  "UPDATE Setting SET Value='2' WHERE Name='DBSchemaVersion'";
                updateCommand.ExecuteNonQuery();
            }
        }

        private void CreateDatabase(string dbDir)
        {
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            string fullDbPath = Path.Combine(dbDir, Constants.DatabaseName);
            if (!File.Exists(fullDbPath))
            {
                File.Copy(Constants.DatabaseName, fullDbPath);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_viewModel != null)
            {
                _viewModel.Dispose();
            }
        }
    }
}