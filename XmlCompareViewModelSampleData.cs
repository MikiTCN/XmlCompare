using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace XmlCompare
{
    public class XmlCompareViewModelSampleData : XmlCompareViewModel
    {
        public XmlCompareViewModelSampleData()
        {
            DiffList = new ObservableCollection<AttributeChangeViewModel>()
            {
                new AttributeChangeViewModel(){ID="eID_COMMAND",Name="IsParameter",From="1",To=null},
                new AttributeChangeViewModel(){ID="eID_COMMAND",Name="Name",From="Command",To="Commanded"},
                new AttributeChangeViewModel(){ID="eID_COMMAND",Name="IdName",From="",To="23ABE2412"},
            };
        }
    }
}