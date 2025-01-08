using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace XmlCompare
{
    /// <summary>
    /// Compare attributes where ID is in attribute
    /// Or Compare Elements where ID is in element
    /// Finding match by ID, Producing DiffList of attribute/element changes
    /// </summary>
    public class XmlCompareViewModel : INotifyPropertyChanged
    {
        private string _compareFileName;
        private string _compareToFileName;
        private ObservableCollection<AttributeChangeViewModel> _diffList;
        private string _compareName;
        private string _compareToName;
        private const string initialElementName = "Register";
        private string _elementName = initialElementName;
        private string _idName = "ID";
        private List<string> _elementNames = new List<string> { initialElementName, "ParameterItem" };

        public string CompareFileName
        {
            get => _compareFileName;
            set
            {
                _compareFileName = value;
                NotifyPropertyChanged();
            }
        }

        public string CompareName
        {
            get => _compareName;
            set
            {
                _compareName = value;
                NotifyPropertyChanged();
            }
        }

        public string CompareToFileName
        {
            get => _compareToFileName;
            set
            {
                _compareToFileName = value;
                NotifyPropertyChanged();
            }
        }

        public string CompareToName
        {
            get => _compareToName;
            set
            {
                _compareToName = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<AttributeChangeViewModel> DiffList
        {
            get => _diffList;
            set
            {
                _diffList = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasCompared { get; set; }

        public Dictionary<string, XElement> Xml1Dictionary { get; set; }
        public Dictionary<string, XElement> Xml2Dictionary { get; set; }

        public string ElementName
        {
            get => _elementName;
            set
            {
                _elementName = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> ElementNames
        {
            get => _elementNames;
            set
            {
                _elementNames = value;
                NotifyPropertyChanged();
            }
        }

        public string IdName
        {
            get => _idName;
            set
            {
                _idName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>Implement INotifyPropertyChanged</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify observers that property with a given name has changed
        /// to implement INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">The name of the property, default value is name of caller member</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void CompareFiles()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.CompareFileName) && !string.IsNullOrEmpty(this.CompareToFileName))
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    XDocument xml1;
                    XDocument xml2;
                    List<string> a, b;
                    using (FileStream fs = new FileStream(this.CompareFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        xml1 = XDocument.Load(fs);
                        var registers = from r in xml1.Root?.Descendants(ElementName) where r.Attribute(IdName) != null select r;
                        var decendants = xml1.Root.Elements();
                        this.Xml1Dictionary = new Dictionary<string, XElement>();
                        foreach (var register in registers)
                        {
                            var registerId = register.Attribute(IdName).Value;
                            if (this.Xml1Dictionary.ContainsKey(registerId))
                                throw new Exception(
                                    $"{registerId} exists twice in XML {this.CompareName}, can not open");
                            this.Xml1Dictionary.Add(registerId, register);
                        }

                        a = (from r in registers select r.Attribute(IdName).Value).ToList();
                    }

                    using (FileStream fs = new FileStream(this.CompareToFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        xml2 = XDocument.Load(fs);
                        var registers = from r in xml2.Root?.Descendants(ElementName) where r.Attribute(IdName) != null select r;
                        b = (from r in registers select r.Attribute(IdName).Value).ToList();
                        this.Xml2Dictionary = new Dictionary<string, XElement>();
                        foreach (var register in registers)
                        {
                            var registerId = register.Attribute(IdName).Value;
                            if (this.Xml2Dictionary.ContainsKey(registerId))
                                throw new Exception(
                                    $"{registerId} exists twice in XML {this.CompareToName}, can not open");
                            this.Xml2Dictionary.Add(registerId, register);
                        }
                    }

                    var xml1Registers = from r in xml1.Root?.Descendants(ElementName) where r.Attribute(IdName) != null select r;

                    foreach (var xml1Register in xml1Registers)
                    {
                        if (this.Xml2Dictionary.TryGetValue(xml1Register.Attribute(IdName).Value, out var x2))
                            AddCompair(xml1Register, x2);
                        else
                            AddMissingCompareRemoved(xml1Register);
                    }
                    var xml2Registers = from r in xml2.Root?.Descendants(ElementName) where r.Attribute(IdName) != null select r;
                    foreach (var xml2Register in xml2Registers)
                    {
                        if (!this.Xml1Dictionary.TryGetValue(xml2Register.Attribute(IdName).Value, out var x1)) 
                            AddMissingCompareAdded(xml2Register);
                    }
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception exception)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Failed to load the WXM file\r\n" +
                                $"Exception {exception.GetType().Name}: {exception.Message}\r\n" +
                                $"{exception.GetBaseException().GetType().Name}: {exception.Message}");
            }
        }

        public void AddMissingCompareRemoved(XElement xml1Register)
        {
            var CANIndexFrom = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var CANIndexTo = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var MBIndexFrom = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            var MBIndexTo = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            this.DiffList.Add(new AttributeChangeViewModel(){ID=xml1Register.Attribute(IdName)?.Value, Name = "Removed",
                CANIndexFrom = CANIndexFrom?.Value,
                CANIndexTo = CANIndexTo?.Value,
                MBIndexFrom = MBIndexFrom?.Value,
                MBIndexTo = MBIndexTo?.Value
            });
        }

        public void AddMissingCompareAdded(XElement xml1Register)
        {
            var CANIndexFrom = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var CANIndexTo = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var MBIndexFrom = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            var MBIndexTo = xml1Register.Attributes().FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            this.DiffList.Add(new AttributeChangeViewModel() { ID = xml1Register.Attribute(IdName)?.Value, Name = "Added",
                CANIndexFrom = CANIndexFrom?.Value,
                CANIndexTo = CANIndexTo?.Value,
                MBIndexFrom = MBIndexFrom?.Value,
                MBIndexTo = MBIndexTo?.Value
            });
        }

        public void AddCompair(XElement xml1Register, XElement x2)
        {
            var x1Attrib = xml1Register.Attributes().ToList();
            var x2Attrib = x2.Attributes().ToList();
            var CANIndexFrom = x1Attrib.FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var CANIndexTo = x2Attrib.FirstOrDefault(x => x.Name.LocalName == "CANIndex");
            var MBIndexFrom = x1Attrib.FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            var MBIndexTo = x2Attrib.FirstOrDefault(x => x.Name.LocalName == "MBIndex");
            foreach (var x1Attribute in x1Attrib)
            {
                var x2Attribute = x2Attrib.FirstOrDefault(x => x.Name == x1Attribute.Name);
                if (x2Attribute != null)
                {
                    if(x1Attribute.Value != x2Attribute.Value)
                    {
                        this.DiffList.Add(new AttributeChangeViewModel
                        {
                            ID = xml1Register.Attribute(IdName).Value, Name = x1Attribute.Name.LocalName,
                            From = x1Attribute.Value,
                            To = x2Attribute.Value,
                            CANIndexFrom = CANIndexFrom?.Value,
                            CANIndexTo = CANIndexTo?.Value,
                            MBIndexFrom = MBIndexFrom?.Value,
                            MBIndexTo = MBIndexTo?.Value
                        });
                    }
                    HasCompared = true;
                }
                else
                {
                    this.DiffList.Add(new AttributeChangeViewModel
                    {
                        ID = xml1Register.Attribute(IdName).Value,
                        Name = x1Attribute.Name.LocalName,
                        From = x1Attribute.Value,
                        To = null,
                        CANIndexFrom = CANIndexFrom?.Value,
                        CANIndexTo = CANIndexTo?.Value,
                        MBIndexFrom = MBIndexFrom?.Value,
                        MBIndexTo = MBIndexTo?.Value
                    });
                }

            }

            foreach (var x2Attribute in x2Attrib)
            {
                var x1Attribute = x1Attrib.FirstOrDefault(x => x.Name == x2Attribute.Name);
                if(x1Attribute == null)
                {
                    this.DiffList.Add(new AttributeChangeViewModel
                    {
                        ID = xml1Register.Attribute(IdName).Value,
                        Name = x2Attribute.Name.LocalName,
                        From = null,
                        To = x2Attribute.Value,
                        CANIndexFrom = CANIndexFrom?.Value,
                        CANIndexTo = CANIndexTo?.Value,
                        MBIndexFrom = MBIndexFrom?.Value,
                        MBIndexTo = MBIndexTo?.Value
                    });
                }
            }
        }

        public void CompareFilesElements()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.CompareFileName) && !string.IsNullOrEmpty(this.CompareToFileName))
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    XDocument xml1;
                    XDocument xml2;
                    List<string> a, b;
                    using (FileStream fs = new FileStream(this.CompareFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        xml1 = XDocument.Load(fs);
                        var registers = from r in xml1.Root?.Descendants(ElementName) where r.Element(IdName) != null && r.Element(IdName).Value != null select r;
                        var decendants = xml1.Root.Elements();
                        // do not remove result from CompaireFiles looking for IdName attribute this.Xml1Dictionary = new Dictionary<string, XElement>();
                        foreach (var register in registers)
                        {
                            var registerId = register.Element(IdName).Value;
                            if (this.Xml1Dictionary.ContainsKey(register.Parent.Name.LocalName + registerId))
                                throw new Exception(
                                    $"{register.Parent.Name.LocalName + registerId} exists twice in XML {this.CompareName}, can not open");
                            // Include parent name, so ParameterItems and LiveParameters can both contain Date / Time
                            this.Xml1Dictionary.Add(register.Parent.Name.LocalName + registerId, register);
                        }

                        a = (from r in registers select r.Element(IdName).Value).ToList();
                    }

                    using (FileStream fs = new FileStream(this.CompareToFileName, FileMode.Open, FileAccess.ReadWrite))
                    {
                        xml2 = XDocument.Load(fs);
                        var registers = from r in xml2.Root?.Descendants(ElementName) where r.Element(IdName) != null && r.Element(IdName).Value != null select r;
                        b = (from r in registers select r.Element(IdName).Value).ToList();
                        // do not remove result from CompaireFiles looking for IdName attribute  this.Xml2Dictionary = new Dictionary<string, XElement>();
                        foreach (var register in registers)
                        {
                            var registerId = register.Element(IdName).Value;
                            if (this.Xml2Dictionary.ContainsKey(register.Parent.Name.LocalName + registerId))
                                throw new Exception(
                                    $"{register.Parent.Name.LocalName + registerId} exists twice in XML {this.CompareToName}, can not open");
                            // Include parent name, so ParameterItems and LiveParameters can both contain Date / Time
                            this.Xml2Dictionary.Add(register.Parent.Name.LocalName + registerId, register);
                        }
                    }

                    var xml1Registers = from r in xml1.Root?.Descendants(ElementName) where r.Element(IdName) != null && r.Element(IdName).Value != null select r;

                    foreach (var xml1Register in xml1Registers)
                    {
                        if (this.Xml2Dictionary.TryGetValue(xml1Register.Parent.Name.LocalName + xml1Register.Element(IdName).Value, out var x2))
                            AddCompairElements(xml1Register, x2);
                        else
                            AddMissingCompareElementRemoved(xml1Register);
                    }
                    var xml2Registers = from r in xml2.Root?.Descendants(ElementName) where r.Element(IdName) != null && r.Element(IdName).Value != null select r;
                    foreach (var xml2Register in xml2Registers)
                    {
                        if (!this.Xml1Dictionary.TryGetValue(xml2Register.Parent.Name.LocalName + xml2Register.Element(IdName).Value, out var x1))
                            AddMissingCompareElementAdded(xml2Register);
                    }
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception exception)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Failed to load the WXM file\r\n" +
                                $"Exception {exception.GetType().Name}: {exception.Message}\r\n" +
                                $"{exception.GetBaseException().GetType().Name}: {exception.Message}");
            }
        }

        public void AddMissingCompareElementRemoved(XElement xml1Register)
        {
            var CANIndexFrom = xml1Register.Elements().FirstOrDefault(x => x.Name.LocalName == "Name");
            var CANIndexTo = xml1Register.Elements().FirstOrDefault(x => x.Name.LocalName == "Name");
            var MBIndexFrom = xml1Register.Parent;
            var MBIndexTo = xml1Register.Parent;
            this.DiffList.Add(new AttributeChangeViewModel()
            {
                ID = xml1Register.Element(IdName)?.Value,
                Name = "Removed",
                CANIndexFrom = CANIndexFrom?.Value,
                CANIndexTo = CANIndexTo?.Value,
                MBIndexFrom = MBIndexFrom?.Name.LocalName,
                MBIndexTo = MBIndexTo?.Name.LocalName
            });
        }

        public void AddMissingCompareElementAdded(XElement xml1Register)
        {
            var CANIndexFrom = xml1Register.Elements().FirstOrDefault(x => x.Name.LocalName == "Name");
            var CANIndexTo = xml1Register.Elements().FirstOrDefault(x => x.Name.LocalName == "Name");
            var MBIndexFrom = xml1Register.Parent;
            var MBIndexTo = xml1Register.Parent;
            this.DiffList.Add(new AttributeChangeViewModel()
            {
                ID = xml1Register.Element(IdName)?.Value,
                Name = "Added",
                CANIndexFrom = CANIndexFrom?.Value,
                CANIndexTo = CANIndexTo?.Value,
                MBIndexFrom = MBIndexFrom?.Name.LocalName,
                MBIndexTo = MBIndexTo?.Name.LocalName
            });
        }

        public void AddCompairElements(XElement xml1Register, XElement x2)
        {
            var x1Elements = xml1Register.Elements().ToList();
            var x2Elements = x2.Elements().ToList();
            var CANIndexFrom = x1Elements.FirstOrDefault(x => x.Name.LocalName == "Name");
            var CANIndexTo = x2Elements.FirstOrDefault(x => x.Name.LocalName == "Name");
            var MBIndexFrom = xml1Register.Parent;
            var MBIndexTo = x2.Parent;
            foreach (var x1Element in x1Elements)
            {
                var x2Element = x2Elements.FirstOrDefault(x => x.Name == x1Element.Name);
                if (x2Element != null)
                {
                    if (x1Element.Value != x2Element.Value)
                    {
                        this.DiffList.Add(new AttributeChangeViewModel
                        {
                            ID = xml1Register.Element(IdName).Value,
                            Name = x1Element.Name.LocalName,
                            From = x1Element.Value,
                            To = x2Element.Value,
                            CANIndexFrom = CANIndexFrom?.Value,
                            CANIndexTo = CANIndexTo?.Value,
                            MBIndexFrom = MBIndexFrom?.Name.LocalName,
                            MBIndexTo = MBIndexTo?.Name.LocalName
                        });
                    }
                    HasCompared = true;
                }
                else
                {
                    this.DiffList.Add(new AttributeChangeViewModel
                    {
                        ID = xml1Register.Element(IdName).Value,
                        Name = x1Element.Name.LocalName,
                        From = x1Element.Value,
                        To = null,
                        CANIndexFrom = CANIndexFrom?.Value,
                        CANIndexTo = CANIndexTo?.Value,
                        MBIndexFrom = MBIndexFrom?.Name.LocalName,
                        MBIndexTo = MBIndexTo?.Name.LocalName
                    });
                }

            }

            foreach (var x2Element in x2Elements)
            {
                var x1Element = x1Elements.FirstOrDefault(x => x.Name == x2Element.Name);
                if (x1Element == null)
                {
                    this.DiffList.Add(new AttributeChangeViewModel
                    {
                        ID = xml1Register.Element(IdName).Value,
                        Name = x2Element.Name.LocalName,
                        From = null,
                        To = x2Element.Value,
                        CANIndexFrom = CANIndexFrom?.Value,
                        CANIndexTo = CANIndexTo?.Value,
                        MBIndexFrom = MBIndexFrom?.Name.LocalName,
                        MBIndexTo = MBIndexTo?.Name.LocalName
                    });
                }
            }
        }


    }
}