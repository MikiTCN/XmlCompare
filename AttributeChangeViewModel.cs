using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace XmlCompare
{
    /// <summary>
    /// For Diff list show 1 changed attribute value or 1 child Element value
    /// </summary>
    public class AttributeChangeViewModel
    {
        public string ID { get; set; }
        /// <summary>
        /// For Canbus parameter files show ID as hex so it is recognized from documentation
        /// </summary>
        public string IDHex
        {
            get { return int.TryParse(ID, out var idNumber) ? string.Format("0x{0:X6}", idNumber) : ""; }
        }
        public string Name { get; set; }

        public string From { get; set; }

        public string To { get; set; }
        /// <summary>
        /// CanIndex value for WXM or Name element for ParameterItem
        /// </summary>
        public string CANIndexFrom { get; set; }
        public string CANIndexTo { get; set; }
        /// <summary>
        /// MBIndex value for WXM or parent element name of ParameterItem (ParameterItems or LiveParameters)
        /// </summary>
        public string MBIndexFrom { get; set; }
        public string MBIndexTo { get; set; }
    }
}
