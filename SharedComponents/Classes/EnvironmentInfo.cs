using System;
using System.Collections.Generic;
using System.Text;

namespace FinalBuild
{
    public enum eEnvironmentName
    {
        Live,
        Development,
        Test,
        UAT,
        Snapshot,
        Training
    }

    [Serializable]
    public class EnvironmentInfo
    {
        private eEnvironmentName _name;
        private string _appServer;
        private string _sqlServer;
        private string _iisServer;
        private BusinessObjects.SerializableHashTable _otherServers;

        public eEnvironmentName Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string AppServer
        {
            get
            {
                return _appServer;
            }
            set
            {
                _appServer = value;
            }
        }
        public string SQLServer
        {
            get
            {
                return _sqlServer;
            }
            set
            {
                _sqlServer = value;
            }
        }
        public string IISServer
        {
            get
            {
                return _iisServer;
            }
            set
            {
                _iisServer = value;
            }
        }
        public BusinessObjects.SerializableHashTable OtherServers
        {
            get
            {
                return _otherServers;
            }
            set
            {
                _otherServers = value;
            }
        }
    }
}
