using System;
using System.Collections.Generic;
using MuhasebeStokWebApp.ViewModels.Shared;

namespace MuhasebeStokWebApp.ViewModels.Irsaliye
{
    public class IrsaliyeListViewModel
    {
        public List<IrsaliyeViewModel> Irsaliyeler { get; set; } = new List<IrsaliyeViewModel>();
        public PagingInfo PagingInfo { get; set; } = new PagingInfo();
        public string SearchString { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
    }
} 