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
namespace SophisETL.Transform.Comment.Xml {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    [System.Xml.Serialization.XmlRootAttribute("settings", Namespace="http://www.sophis.net/SophisETL/Transform/Comment", IsNullable=false)]
    public partial class Settings {
        
        private Comment[] commentField;
        
        private MultiComment[] complexCommentField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("comment")]
        public Comment[] comment {
            get {
                return this.commentField;
            }
            set {
                this.commentField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("complexComment")]
        public MultiComment[] complexComment {
            get {
                return this.complexCommentField;
            }
            set {
                this.complexCommentField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public partial class Comment {
        
        private SimpleMatch[] simpleMatchField;
        
        private string sourceFieldField;
        
        private string targetFieldField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("simpleMatch")]
        public SimpleMatch[] simpleMatch {
            get {
                return this.simpleMatchField;
            }
            set {
                this.simpleMatchField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sourceField {
            get {
                return this.sourceFieldField;
            }
            set {
                this.sourceFieldField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public partial class SimpleMatch {
        
        private string matchField;
        
        private Comparator comparatorField;
        
        private Quote actionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string match {
            get {
                return this.matchField;
            }
            set {
                this.matchField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Comparator comparator {
            get {
                return this.comparatorField;
            }
            set {
                this.comparatorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Quote action {
            get {
                return this.actionField;
            }
            set {
                this.actionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public enum Comparator {
        
        /// <remarks/>
        equal,
        
        /// <remarks/>
        greater,
        
        /// <remarks/>
        smaller,
        
        /// <remarks/>
        different,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public enum Quote {
        
        /// <remarks/>
        beginComment,
        
        /// <remarks/>
        endComment,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public partial class MultiMatch {
        
        private string sourceFieldField;
        
        private string matchField;
        
        private Comparator comparatorField;
        
        private Operand operandField;
        
        private bool operandFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sourceField {
            get {
                return this.sourceFieldField;
            }
            set {
                this.sourceFieldField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string match {
            get {
                return this.matchField;
            }
            set {
                this.matchField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Comparator comparator {
            get {
                return this.comparatorField;
            }
            set {
                this.comparatorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Operand operand {
            get {
                return this.operandField;
            }
            set {
                this.operandField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool operandSpecified {
            get {
                return this.operandFieldSpecified;
            }
            set {
                this.operandFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public enum Operand {
        
        /// <remarks/>
        And,
        
        /// <remarks/>
        Or,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sophis.net/SophisETL/Transform/Comment")]
    public partial class MultiComment {
        
        private MultiMatch[] multiMatchField;
        
        private Quote actionField;
        
        private string targetFieldField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MultiMatch")]
        public MultiMatch[] MultiMatch {
            get {
                return this.multiMatchField;
            }
            set {
                this.multiMatchField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Quote action {
            get {
                return this.actionField;
            }
            set {
                this.actionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string targetField {
            get {
                return this.targetFieldField;
            }
            set {
                this.targetFieldField = value;
            }
        }
    }
}
