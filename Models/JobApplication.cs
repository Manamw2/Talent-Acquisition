﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class JobApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign Key to AppUser

        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; } // Navigation Property

        // Remove JobId, add BatchId
        [Required]
        public int BatchId { get; set; }

        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        // Optional: Keep reference to Job for maintaining existing data relationships
        /*public int? JobId { get; set; }

        [ForeignKey("JobId")]
        public Job? Job { get; set; }*/

        [Required]
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public string? AddedBy { get; set; }
        public string? SourceDetails { get; set; }
        public string? Source { get; set; }
    }
    public enum Status
    {
        Applied,
        UnderReviewing,
        NotMached,
        Pending
    }
}
