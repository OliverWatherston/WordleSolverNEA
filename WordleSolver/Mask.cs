using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public class Mask : List<char>
    {
        private List<List<char>> _mask = new List<List<char>>();
        public Mask(int size)
        {
            for (int i = 0; i < size; i++)
            {
                _mask.Add(new List<char>());
            }
        }

        public void AddToAllPositionMasks(IEnumerable<char> enumerableCharList, int startIndex = 0, int count = -1)
        {
            int maskCount = (-1 == count) ? _mask.Count : count; // since optional parameters are compile time only we use a placeholder and replace it in runtime if we don't change it
            for (int i = startIndex; i < maskCount; i++)
            {
                var positionMask = _mask[i];
                positionMask.AddRange(enumerableCharList);
            }
        }

        public void AddToPositionMask(IEnumerable<char> enumerableCharList, int index)
        {
            _mask[index].AddRange(enumerableCharList);
        }

        public List<List<char>> GetMask()
        {
            return _mask;
        }

        public IEnumerable<char> GetAllUnique()
        {
            // we use and add to a hashset since each value in a hashset MUST be unique, this means we do not have to check for unique values ourselves
            var ret = new HashSet<char>();
            _mask.ForEach(positionMask => {
                positionMask.ForEach(letter => ret.Add(letter));
            });
            return ret.ToList();
        }

        public List<char> GetIndex(int index)
        {
            return _mask[index];
        }
        
        public void CopyOtherMask(Mask mask)
        {
            _mask = mask.GetMask().ConvertAll(positionMask => new List<char>(positionMask));
        }

        public int GetCountOfAllPositionMasks()
        {
            return _mask.Count(x => x.Count > 0);
        }

        public void AddToPositionMaskUnique(int index, char t)
        {
            var positionMask = _mask[index];
            if (!positionMask.Contains(t))
            {
                positionMask.Add(t);
            }
        }

        public void RemoveFromPositionMask(int index, char t)
        {
            var positionMask = _mask[index];
            if (positionMask.Contains(t))
            {
                positionMask.Remove(t);
            }
        }
    }
}
