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
namespace SophisETL.Transform.Velocity.Xml {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Velocity")]
    [System.Xml.Serialization.XmlRootAttribute("settings", Namespace="http://www.sophis.net/SophisETL/Transform/Velocity", IsNullable=false)]
    public partial class Settings {
        
        private Template templateField;
        
        private string targetFieldField;
        
        /// <remarks/>
        public Template template {
            get {
                return this.templateField;
            }
            set {
                this.templateField = value;
            }
        }
        
        /// <remarks/>
        public string targetField {
            get {
                return this.targetFieldField;
            }
            set {
                this.targetFieldField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Velocity")]
    public partial class Template {
        
        private TemplateKindEnum kindField;
        
        private string valueField;
        
        public Template() {
            this.kindField = TemplateKindEnum.file;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(TemplateKindEnum.file)]
        public TemplateKindEnum kind {
            get {
                return this.kindField;
            }
            set {
                this.kindField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Velocity")]
    public enum TemplateKindEnum {
        
        /// <remarks/>
        file,
        
        /// <remarks/>
        field,
        
        /// <remarks/>
        inline,
    }
}
