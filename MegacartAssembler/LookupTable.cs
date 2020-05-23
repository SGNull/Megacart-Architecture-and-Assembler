using System.Collections.Generic;

namespace MegacartAssembler
{
    class LookupTable
    {
        private List<TableEntry> _table = new List<TableEntry>();

        public List<TableEntry> Table
        {
            get => _table;
            set => _table = value;
        }

        private string Name { get; set; }

        public LookupTable(string name)
        {
            Name = name;
        }

        public void AddEntry(TableEntry newEntry)
        {
            Table.Add(newEntry);
        }

        public string GetValueForKeyword(string keyword)
        {
            foreach (TableEntry entry in Table)
            {
                if (entry.HasKeyword(keyword))
                {
                    return entry.Value;
                }
            }

            throw new KeyNotFoundException("The keyword: " + keyword + " was not found in " + Name);
        }

        public bool HasEntryWithKeyword(string keyword)
        {
            foreach (TableEntry entry in Table)
            {
                if (entry.Keyword == keyword)
                    return true;
            }

            return false;
        }
        public int GetPositionOfKeyword(string keyword)
        {
            for (int index = 0; index < Table.Count; index++)
            {
                if (Table[index].HasKeyword(keyword))
                    return index;
            }
            throw new KeyNotFoundException("The keyword: " + keyword + " was not found in " + Name);
        }
    }
}