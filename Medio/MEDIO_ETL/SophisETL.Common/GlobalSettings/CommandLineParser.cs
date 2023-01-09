using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SophisETL.Common.GlobalSettings
{
    internal class CommandLineParser
    {
        private Dictionary<string, CommandLineItem> _Items;

        public CommandLineParser() { }

        public void Parse( string[] commandLine )
        {
            _Items = new Dictionary<string, CommandLineItem>();
            CommandLineItem currentItem = null;

            foreach ( string item in commandLine )
            {
                // Is it an option ? -> start a new Item
                if ( item.StartsWith( "-" ) )
                {
                    currentItem = new CommandLineItem();
                    currentItem.Option = item.Substring( 1 );
                    _Items.Add( currentItem.Option, currentItem );
                }
                // else it is a value but let's check that
                else if ( currentItem != null )
                {
                    currentItem.Value = item;
                    currentItem = null; // current item has a value, we close it
                }
                else
                {
                    throw new Exception( "Command line error: " + item + " not recognized" );
                }
            }
        }

        public Dictionary<string, CommandLineItem> Items { get { return _Items; } }
    }

    public class CommandLineItem
    {
        public string Option { get; set; }
        public string Value  { get; set; }
    }
}
