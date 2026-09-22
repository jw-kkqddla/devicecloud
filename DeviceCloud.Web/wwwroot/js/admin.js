// 后台管理脚本

// 切换侧边栏
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const mainContainer = document.getElementById('mainContainer');
    const navbar = document.getElementById('navbar');

    sidebar.classList.toggle('collapsed');
    mainContainer.classList.toggle('sidebar-collapsed');
    navbar.classList.toggle('sidebar-collapsed');
}

// 全屏切换
function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
    } else {
        document.exitFullscreen();
    }
}

// 菜单展开/折叠
document.addEventListener('DOMContentLoaded', function() {
    // 处理菜单点击
    const menuItems = document.querySelectorAll('.sidebar-menu > .menu-item > a');

    menuItems.forEach(function(item) {
        item.addEventListener('click', function(e) {
            const parent = this.parentElement;
            const hasSubmenu = parent.querySelector('.submenu');

            if (hasSubmenu) {
                e.preventDefault();
                // 手风琴效果：展开当前项前，先关闭其他兄弟子菜单
                Array.from(parent.parentElement.children).forEach(function(sib) {
                    if (sib !== parent) {
                        sib.classList.remove('open');
                    }
                });
                parent.classList.toggle('open');
            }
        });
    });

    // 移动端菜单
    const hamburger = document.getElementById('hamburger');
    if (hamburger) {
        hamburger.addEventListener('click', function() {
            if (window.innerWidth <= 768) {
                document.getElementById('sidebar').classList.toggle('mobile-show');
            }
        });
    }
});

// 表格全选
function toggleSelectAll(checkbox) {
    const checkboxes = document.querySelectorAll('tbody input[type="checkbox"]');
    checkboxes.forEach(function(cb) {
        cb.checked = checkbox.checked;
    });
}

// 确认删除
function confirmDelete(message, callback) {
    if (confirm(message || '确定要删除吗？')) {
        callback && callback();
    }
}

// 显示消息提示
function showMessage(message, type) {
    type = type || 'info';

    const alert = document.createElement('div');
    alert.className = 'alert alert-' + (type === 'error' ? 'danger' : type);
    alert.style.cssText = 'position:fixed;top:60px;right:20px;z-index:9999;min-width:200px;';
    alert.innerHTML = message;

    document.body.appendChild(alert);

    setTimeout(function() {
        alert.remove();
    }, 3000);
}
