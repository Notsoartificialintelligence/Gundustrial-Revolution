using System.IO;
using System.Linq;

namespace PrefabAPI
{
    public class EditorObject
    {
        public long m_PathID;
        public int[] version;
        public EditorClassIDType type;
        public SerializedType serializedType;
        public uint byteSize;

        public EditorObject()
        {
        }

        public virtual void Write(BinaryWriter write)
        {
        }

        protected bool HasStructMember(string name)
        {
            return serializedType?.m_Nodes != null && serializedType.m_Nodes.Any(x => x.m_Name == name);
        }
    }
}
