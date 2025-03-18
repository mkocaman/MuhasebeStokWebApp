using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.Data.Entities;

namespace MuhasebeStokWebApp.ViewModels.SistemLog
{
    public class SistemLogListViewModel
    {
        public List<MuhasebeStokWebApp.Data.Entities.SistemLog> Logs { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
} 