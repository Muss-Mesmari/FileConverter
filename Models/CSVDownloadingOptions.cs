using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
    public enum CSVDownloadingOptions
    {
        None,
        Tables,
        Objects,
        Relationships,
        DownloadAllTables,
        DownloadAllObjects,
        AttributesInInputMessage,
        AttributesInOutputMessage,
        AttributesGroupsInInputMessage,
        AttributesGroupsOutInputMessage,
        RelationshipsToAttributesOrAttributesGroupsInputMessage,
        RelationshipsToAttributesOrAttributesGroupsOutputMessage
    }
}
