using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenDdDemo.Model
{
	public class Product : EntityBase
	{
		public int Price { get; set; }
		public string Description { get; set; }
		public string CategoryId { get; set; }
	}
}
