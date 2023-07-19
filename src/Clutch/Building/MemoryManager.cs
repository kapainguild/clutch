using System.Collections.Generic;
using System.Text.Json;

namespace Clutch.Building
{
    public class MemoryManager
    {
        private readonly List<MemoryHandle> _handles = new List<MemoryHandle>();
        private int _arrayRequestedCount;

        public MemoryHandle Allocate(string value)
        {
            var result = new MemoryHandle(this, value);
            _handles.Add(result);
            return result;
        }

        public void ArrayRequested()
        {
            foreach (var memoryHandle in _handles)
            {
                memoryHandle.EncodedText = JsonEncodedText.Encode(memoryHandle.Value);
            }
            _handles.Clear();
            _arrayRequestedCount++;
            if (_arrayRequestedCount != 1)
                throw new ClutchInternalErrorException("ArrayRequested more then once");
        }
    }

    public class MemoryHandle
    {
        private readonly MemoryManager _memoryManager;
        private JsonEncodedText _encodedText;
        private bool _allocatedInPool;

        public MemoryHandle(MemoryManager memoryManager, string value)
        {
            _memoryManager = memoryManager;
            Value = value;
        }

        public string Value { get; set; }

        public JsonEncodedText EncodedText
        {
            get
            {
                if (!_allocatedInPool)
                    _memoryManager.ArrayRequested();
                return _encodedText;
            }
            set
            {
                _allocatedInPool = true;
                _encodedText = value;
            }
        }
    }
}
