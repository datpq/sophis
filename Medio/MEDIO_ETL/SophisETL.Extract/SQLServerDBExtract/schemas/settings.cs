﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 
namespace SophisETL.Extract.SQLServerDBExtract.Xml {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Extract/SQLServerDBExtract")]
    [System.Xml.Serialization.XmlRootAttribute("settings", Namespace="http://www.sophis.net/SophisETL/Extract/SQLServerDBExtract", IsNullable=false)]
    public partial class Settings {
        
        private DBConnection dbConnectionField;
        
        private int connectionTimeoutField;
        
        private int queryTimeoutField;
        
        private string queryField;
        
        public Settings() {
            this.connectionTimeoutField = 15;
            this.queryTimeoutField = 30;
        }
        
        /// <remarks/>
        public DBConnection dbConnection {
            get {
                return this.dbConnectionField;
            }
            set {
                this.dbConnectionField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(15)]
        public int connectionTimeout {
            get {
                return this.connectionTimeoutField;
            }
            set {
                this.connectionTimeoutField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(30)]
        public int queryTimeout {
            get {
                return this.queryTimeoutField;
            }
            set {
                this.queryTimeoutField = value;
            }
        }
        
        /// <remarks/>
        public string query {
            get {
                return this.queryField;
            }
            set {
                this.queryField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Extract/SQLServerDBExtract")]
    public partial class DBConnection {
        
        private string loginField;
        
        private string passwordField;
        
        private string instanceField;
        
        /// <remarks/>
        public string login {
            get {
                return this.loginField;
            }
            set {
                this.loginField = value;
            }
        }
        
        /// <remarks/>
        public string password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        /// <remarks/>
        public string instance {
            get {
                return this.instanceField;
            }
            set {
                this.instanceField = value;
            }
        }
    }
}