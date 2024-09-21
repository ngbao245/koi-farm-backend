﻿using Repository.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Repository.Data.Entity
{
    [Table("Product")]
    public class Product : Entity    
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Quantity { get; set; }


        // Navigation property for related Product_Item entities
        public ICollection<ProductItem> ProductItems { get; set; }

    }
}
