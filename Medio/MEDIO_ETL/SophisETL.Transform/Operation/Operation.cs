using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.Operation.Xml;


namespace SophisETL.Transform.Operation
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class Operation : AbstractBasicTransformTemplate
    {
        private Settings _Settings { get; set; }

        public override void Init()
        {            
            base.Init();

            // check the syntax of the incoming XML, if it makes sense or not
            foreach (Xml.Operation oper in _Settings.operations)
            {
                string operand  = oper.operand;
                
                if ( oper.type == TypeEnum.unary )
                {
                    if (String.Compare(operand, "abs", true) != 0) { throw new Exception("Error: Operand is not a correct unary operand: " + operand); }
                }
                else if ( oper.type == TypeEnum.binary )
                {
                    // check the operand
                    if (String.Compare(operand, "+") != 0 &&
                        String.Compare(operand, "-") != 0 &&
                        String.Compare(operand, "*") != 0 &&
                        String.Compare(operand, "/") != 0)
                    {
                        throw new Exception("Error: Operand is not a correct binary operand: " + operand);
                    }
                }
                else {throw new Exception("Error: this type is neither binary nor unary: " + oper.type.ToString());}
            }
        }

        protected override Record Transform( Record record )
        {
            LogManager log = LogManager.Instance;
            log.DebugMode = true;

            foreach (Xml.Operation oper in _Settings.operations)
            {
                string field1   = oper.field1;
                string field2   = oper.field2;
                string operand  = oper.operand;
                string result   = oper.result;
                
//                 log.Log("typeOper: "    + typeOper);
//                 log.Log("field1: "      + field1);
//                 log.Log("field2: "      + field2);
//                 log.Log("operand: "     + operand);
//                 log.Log("result: "      + field2);

                double theResult = .0;

                if ( oper.type == TypeEnum.unary )
                {
                    // Extract Field Value
                    double fieldValue = 0.0;
                    if ( record.Fields.ContainsKey( field1 ) )
                    {
                        if ( !IsNumeric( record.Fields[field1], ref fieldValue ) )
                            throw new Exception( "Error: the value is not a number" );
                    }
                    else
                    {
                        switch ( oper.missingFieldAction )
                        {
                            default:
                            case MissingFieldActionEnum.failRecord:
                                return null;
                            case MissingFieldActionEnum.returnZeroValue:
                                theResult = 0.0;
                                goto saveResult; // sorry...
                            case MissingFieldActionEnum.useZeroValue:
                                fieldValue = 0.0;
                                break;
                        }
                    }
                    
                    // ABS
                    if ( String.Compare(operand, "abs", true) == 0 )
                    {
                        // Compute
                        theResult = Math.Abs( fieldValue );
                    }
                }
                else if ( oper.type == TypeEnum.binary )
                {
                    // Extract Field Values
                    double field1Value = 0.0;
                    if ( record.Fields.ContainsKey( field1 ) )
                    {
                        if ( ! IsNumeric( record.Fields[field1], ref field1Value ) )
                            throw new Exception( "Error: the value is not a number" );
                    }
                    else
                    {
                        switch ( oper.missingFieldAction )
                        {
                            default:
                            case MissingFieldActionEnum.failRecord:
                                return null;
                            case MissingFieldActionEnum.returnZeroValue:
                                theResult = 0.0;
                                goto saveResult; // sorry...
                            case MissingFieldActionEnum.useZeroValue:
                                field1Value = 0.0;
                                break;
                        }
                    }

                    double field2Value = 0.0;
                    if ( record.Fields.ContainsKey( field2 ) )
                    {
                        if ( !IsNumeric( record.Fields[field2], ref field2Value ) )
                            throw new Exception( "Error: the value is not a number" );
                    }
                    else
                    {
                        switch ( oper.missingFieldAction )
                        {
                            default:
                            case MissingFieldActionEnum.failRecord:
                                return null;
                            case MissingFieldActionEnum.returnZeroValue:
                                theResult = 0.0;
                                goto saveResult; // sorry...
                            case MissingFieldActionEnum.useZeroValue:
                                field2Value = 0.0;
                                break;
                        }
                    }

                    // operation to do
                    switch (operand)
                    {
                        case "+":
                            theResult = field1Value + field2Value;
                            break;
                        case "-":
                            theResult = field1Value - field2Value;
                            break;
                        case "*":
                            theResult = field1Value * field2Value;
                            break;
                        case "/":
                            if ( field2Value != 0 ) { theResult = field1Value / field2Value; }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    log.Log("this type is not defined: " + oper.type.ToString() );
                }
                
                // if the key already exist in the table, just update it
                saveResult:
                if (record.Fields.ContainsKey(result))
                    record.Fields[result] = theResult;
                else
                    record.Fields.Add(result, theResult);
            }

            return record;
        }       
  
        // check if an object is a numeric type and store the value in dValue
        private bool IsNumeric(object Expression, ref double dValue )
        {
            bool bIsNum = false;
            double dRetNum = .0;
            bIsNum = Double.TryParse(Convert.ToString(Expression),
                                      System.Globalization.NumberStyles.Any,
                                      System.Globalization.NumberFormatInfo.InvariantInfo,
                                      out dRetNum);
            
            dValue = dRetNum;
            return bIsNum;
        }
    }
}
