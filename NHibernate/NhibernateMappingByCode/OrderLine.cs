using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NhibernateMappingByCode
{
	public class OrderLine : Entity
	{
		public virtual int ProductId { get; set; }
		public virtual Order Order { get; set; }
		public virtual int Price { get; set; }
		public virtual string Note { get; set; }
	}
}
