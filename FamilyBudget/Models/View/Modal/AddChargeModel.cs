﻿using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyBudget.ModalViewModels
{
    public class AddChargeModel
    {
        public FinType FinType { get; set; }

        public bool ForAll { get; set; }

        public int Value { get; set; }

        public int CategoryId { get; set; }

        public int? ProjectMemberId { get; set; }
        public int ProjectId { get; set; }
        public bool IsValid { get { return !(IsNotFound || IsForbid); } }
        public bool IsNotFound { get; set; }
        public bool IsForbid { get; set; }
    }
}
