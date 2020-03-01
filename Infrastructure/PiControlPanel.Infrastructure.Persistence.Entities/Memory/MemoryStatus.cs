﻿namespace PiControlPanel.Infrastructure.Persistence.Entities.Memory
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MemoryStatus : BaseTimedEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Used { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Available { get; set; }
    }
}