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
namespace SophisETL.Transform.Merge.Xml {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Merger")]
    [System.Xml.Serialization.XmlRootAttribute("settings", Namespace="http://www.sophis.net/SophisETL/Transform/Merger", IsNullable=false)]
    public partial class Settings {
        
        private SettingsMergeType mergeTypeField;
        
        private JointKey[] keySetListField;
        
        /// <remarks/>
        public SettingsMergeType MergeType {
            get {
                return this.mergeTypeField;
            }
            set {
                this.mergeTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public JointKey[] KeySetList {
            get {
                return this.keySetListField;
            }
            set {
                this.keySetListField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.sophis.net/SophisETL/Transform/Merger")]
    public enum SettingsMergeType {
        
        /// <remarks/>
        InnerJoin,
        
        /// <remarks/>
        LeftJoin,
        
        /// <remarks/>
        OuterJoin,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Merger")]
    public partial class JointKey {
        
        private string leftKeyNameField;
        
        private string rightKeyNameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LeftKeyName {
            get {
                return this.leftKeyNameField;
            }
            set {
                this.leftKeyNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RightKeyName {
            get {
                return this.rightKeyNameField;
            }
            set {
                this.rightKeyNameField = value;
            }
        }
    }
}
