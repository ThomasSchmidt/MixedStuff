using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NhibernateMappingByCode
{
	public class Order : Entity
	{
		public Order()
		{
			OrderLines = new Collection<OrderLine>();
		}
		public virtual DateTime OrderDate { get; set; }
		public virtual ICollection<OrderLine> OrderLines { get; set; }
	}
}
