using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingRoom
{
	public interface ISaveable
	{
		string GetGuid();

		string Save();

		void Load(string state);
	}
}
