# XmlCompare
is used to find what attributes or sub elements have changed for a given element name.<br/>
It finds all elements with a given name (Register or ParameterItem is default, but can be changed)</br>
To match two elements the ID= attribute or &lt;ID&gt; subelement is matched. (The ID name can be changed).</br>

When an element is matched by ID
- it will list all the attribute / subelements that do not match
 with the ID, Name, From, To
- Each attribute/element that has a different value is displayed on a separate line.

If no match for an ID is found, a line with name **Removed** or **Added** is shown.
-  (Right click to see the xml element)

Since it is designed for custom personal use, extra columns are displayed (ID,IDHex,Name,From,To,CANIndexFrom,CANIndexTo,MBIndexFrom,MBIndexTo)

Program was created to compare xml files saved as XML Data (.xml) in Excel,<br/>
as Excel could change the order of columns based when an attribute was discovered when opening a xml file in Excel,<br/> 
and then when saving it as XML Data again the columns would have changed order making file compare impossible.<br/>

Also file compare programs can get out of sync when elements have been added and attributes changed.

It is also useful when comparing different versions of an xml file.

#### Limitations:
Since the elements are added to a dictionary, the ID for each element has to be unique.<br/>
 (Exception is if the elements belong to two different parents, then the same ID can be used under each element parent)<br/>
It does not compare xml structure.<br/>
Only one element name is searched for in the xml files.<br/>

It is not meant as a generic XmlCompare, but very useful when looking for changes.
