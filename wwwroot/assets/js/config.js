var primary = localStorage.getItem("primary") || '#7366ff';
var secondary = localStorage.getItem("secondary") || '#f73164';

window.MuhasebeConfig = {
	// Tema ayarları
	theme: {
		primary: primary,
		secondary: secondary
	},
	
	// Menü ayarları
	menu: {
		animation: true,
		speed: 400,
		keyboard: true
	},
	
	// Sayfa yükleme ayarları
	loader: {
		enabled: true,
		image: '../assets/images/loader.gif',
		text: 'Yükleniyor...'
	},
	
	// Bildirim ayarları
	notification: {
		position: 'top-right',
		timeout: 5000,
		showProgressBar: true
	},
	
	// Tablo ayarları
	datatable: {
		language: {
			url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Turkish.json'
		},
		pageLength: 10,
		responsive: true
	}
};

// Tema renklerini uygula
document.documentElement.style.setProperty('--theme-default', primary);
document.documentElement.style.setProperty('--theme-secondary', secondary);

// defalt layout
$("#default-demo").click(function(){      
    localStorage.setItem('page-wrapper', 'page-wrapper compact-wrapper');
    localStorage.setItem('page-body-wrapper', 'sidebar-icon');
});


// compact layout
$("#compact-demo").click(function(){   
    localStorage.setItem('page-wrapper', 'page-wrapper compact-wrapper compact-sidebar');
    localStorage.setItem('page-body-wrapper', 'sidebar-icon');
});



// modern layout
$("#modern-demo").click(function(){   
    localStorage.setItem('page-wrapper', 'page-wrapper compact-wrapper modern-sidebar');
    localStorage.setItem('page-body-wrapper', 'sidebar-icon');
});