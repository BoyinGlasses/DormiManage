import { useState } from 'react';
import { 
  Search, 
  Home, 
  Calendar, 
  Users, 
  Megaphone, 
  MessageSquare, 
  Bell, 
  ChevronDown, 
  HelpCircle,
  LogOut,
  X,
  UserCheck,
  Check
} from 'lucide-react';
import { User } from '../types';

interface NavbarProps {
  searchQuery: string;
  setSearchQuery: (q: string) => void;
  currentUser: User;
  onUserChange: (user: User) => void;
  usersList: User[];
  onQuickNav: (category: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại' | 'All') => void;
}

export default function Navbar({
  searchQuery,
  setSearchQuery,
  currentUser,
  onUserChange,
  usersList,
  onQuickNav,
}: NavbarProps) {
  const [showUserDropdown, setShowUserDropdown] = useState(false);
  const [showNotificationCenter, setShowNotificationCenter] = useState(false);
  const [showMessagesCenter, setShowMessagesCenter] = useState(false);

  // Initial mock logs/notifications
  const [notifications, setNotifications] = useState([
    { id: 1, title: 'BQL Khu B phản hồi góp ý của bạn', desc: 'Ý kiến về thời gian thu dọn rác đã được tiếp nhận...', read: false, time: '20 phút trước' },
    { id: 2, title: 'Sự kiện Đêm nhạc chuẩn bị bắt đầu', desc: 'Hãy có mặt tại sân sinh hoạt chung lúc 19h30 tối nay nhé!', read: false, time: '2 giờ trước' },
    { id: 3, title: 'Bài viết mới của Đội Tình Nguyện', desc: 'Chiến dịch Mùa hè xanh đã chính thức mở form đăng ký...', read: true, time: 'Hôm qua' },
  ]);

  const [chatMessages, setChatMessages] = useState([
    { id: 1, sender: 'Ban Quản Lý', msg: 'Các bạn phòng 402 chú ý đóng cửa ban công tránh mưa lớn nhé.', read: false, avatar: 'https://images.unsplash.com/photo-1560250097-0b93528c311a?q=80&w=128&auto=format&fit=crop' },
    { id: 2, sender: 'Lan Anh', msg: 'Ăn thử cơm gà nướng chưa? Ngon lắm ý cậu!', read: true, avatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?q=80&w=128&auto=format&fit=crop' },
  ]);

  const handleMarkNotificationsAllRead = () => {
    setNotifications(notifications.map(n => ({ ...n, read: true })));
  };

  const handleClearNotifications = () => {
    setNotifications([]);
  };

  const unreadCount = notifications.filter(n => !n.read).length;
  const unreadMsgCount = chatMessages.filter(m => !m.read).length;

  return (
    <header className="fixed top-0 left-0 w-full z-50 flex justify-between items-center px-4 sm:px-8 h-16 bg-white/95 backdrop-blur-md shadow-sm border-b border-slate-100 flex-wrap sm:flex-nowrap">
      
      {/* Brand logo & shortcut buttons */}
      <div className="flex items-center gap-6 sm:gap-8 justify-between w-full sm:w-auto">
        <h1 
          onClick={() => onQuickNav('All')}
          className="text-2xl font-extrabold font-display text-primary select-none cursor-pointer tracking-tight hover:scale-102 transition-all shrink-0"
        >
          DMForum
        </h1>

        <nav className="hidden md:flex items-center gap-5 shrink-0">
          <button 
            onClick={() => onQuickNav('All')}
            className="text-slate-400 hover:text-primary transition-all p-1.5 rounded-lg hover:bg-slate-50 relative group cursor-pointer"
            title="Trang chủ diễn đàn"
          >
            <Home className="w-5 h-5" />
            <span className="absolute bottom-[-24px] left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] px-2 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">Trang chủ</span>
          </button>

          <button 
            onClick={() => onQuickNav('Tin tức chung')}
            className="text-slate-400 hover:text-primary transition-all p-1.5 rounded-lg hover:bg-slate-50 relative group cursor-pointer"
            title="Tin tức từ Ban Quản Lý"
          >
            <Megaphone className="w-5 h-5" />
            <span className="absolute bottom-[-24px] left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] px-2 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">Tin tức chung</span>
          </button>

          <button 
            onClick={() => onQuickNav('CLB & Sự kiện')}
            className="text-slate-400 hover:text-secondary transition-all p-1.5 rounded-lg hover:bg-slate-50 relative group group-hover:text-secondary cursor-pointer"
            title="Hoạt động và Câu lạc bộ"
          >
            <Users className="w-5 h-5" />
            <span className="absolute bottom-[-24px] left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] px-2 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">CLB & Sự kiện</span>
          </button>

          <a 
            href="#emergency"
            onClick={(e) => {
              e.preventDefault();
              const el = document.getElementById('contact-bql');
              el?.scrollIntoView({ behavior: 'smooth' });
              el?.classList.add('ring-4', 'ring-primary/20');
              setTimeout(() => el?.classList.remove('ring-4', 'ring-primary/20'), 2000);
            }}
            className="text-slate-400 hover:text-red-500 transition-all p-1.5 rounded-lg hover:bg-slate-50 relative group cursor-pointer"
            title="Liên hệ dịch vụ trợ giúp"
          >
            <HelpCircle className="w-5 h-5" />
            <span className="absolute bottom-[-24px] left-1/2 -translate-x-1/2 bg-slate-800 text-white text-[10px] px-2 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">Hỗ trợ khẩn cấp</span>
          </a>
        </nav>
      </div>

      {/* Dynamic real-time Search center */}
      <div className="flex items-center gap-6 justify-center flex-1 max-w-md mx-4 sm:mx-12 shrink-0 sm:shrink">
        <div className="relative w-full">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm kiếm tin tức, thông báo..."
            className="w-full bg-slate-50 border border-slate-100/50 rounded-xl pl-10 pr-9 py-2 focus:bg-white focus:ring-2 focus:ring-primary/10 text-sm outline-none transition-all focus:border-primary-container"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 cursor-pointer"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>
      </div>

      {/* Right icons profile actions */}
      <div className="flex items-center gap-3 sm:gap-4 shrink-0 mt-3 sm:mt-0 justify-end w-full sm:w-auto">
        
        {/* Messages Dropdown */}
        <div className="relative">
          <button 
            onClick={() => {
              setShowMessagesCenter(!showMessagesCenter);
              setShowNotificationCenter(false);
              setShowUserDropdown(false);
            }}
            className={`p-2 rounded-xl transition-all relative cursor-pointer ${
              showMessagesCenter ? 'bg-secondary/10 text-secondary' : 'bg-slate-50 text-slate-500 hover:bg-slate-100'
            }`}
          >
            <MessageSquare className="w-5 h-5" />
            {unreadMsgCount > 0 && (
              <span className="absolute top-1 right-1 w-2 h-2 bg-secondary rounded-full animate-pulse" />
            )}
          </button>

          {showMessagesCenter && (
            <div className="absolute right-0 mt-2.5 w-72 bg-white rounded-2xl p-4 shadow-xl border border-slate-100 z-50 animate-in fade-in slide-in-from-top-2 duration-150">
              <div className="flex justify-between items-center mb-3">
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-400">Trò chuyện nội bộ ({chatMessages.length})</h4>
                <button 
                  onClick={() => setChatMessages(chatMessages.map(m => ({ ...m, read: true })))}
                  className="text-[10px] text-primary hover:underline font-semibold cursor-pointer"
                >
                  Đánh dấu đọc hết
                </button>
              </div>
              <div className="flex flex-col gap-3 max-h-[250px] overflow-y-auto">
                {chatMessages.map((m) => (
                  <div key={m.id} className="flex gap-2.5 items-start p-1.5 rounded-lg hover:bg-slate-50 transition-colors">
                    <img src={m.avatar} alt={m.sender} className="w-8 h-8 rounded-full object-cover" />
                    <div>
                      <div className="flex items-center gap-1.5">
                        <span className="text-xs font-bold text-on-surface">{m.sender}</span>
                        {!m.read && <span className="w-1.5 h-1.5 rounded-full bg-secondary" />}
                      </div>
                      <p className="text-[11px] text-slate-500 leading-snug line-clamp-2 mt-0.5">{m.msg}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Bell notification center */}
        <div className="relative">
          <button 
            onClick={() => {
              setShowNotificationCenter(!showNotificationCenter);
              setShowMessagesCenter(false);
              setShowUserDropdown(false);
            }}
            className={`p-2 rounded-xl transition-all relative cursor-pointer ${
              showNotificationCenter ? 'bg-primary/10 text-primary' : 'bg-slate-50 text-slate-500 hover:bg-slate-100'
            }`}
          >
            <Bell className="w-5 h-5" />
            {unreadCount > 0 && (
              <span className="absolute top-1 right-1 w-2 h-2 bg-primary rounded-full animate-bounce" />
            )}
          </button>

          {showNotificationCenter && (
            <div className="absolute right-0 mt-2.5 w-80 bg-white rounded-2xl p-4 shadow-xl border border-slate-100 z-50 animate-in fade-in slide-in-from-top-2 duration-150">
              <div className="flex justify-between items-center pb-2.5 mb-2.5 border-b border-slate-100">
                <span className="text-xs font-bold uppercase tracking-wider text-slate-400">Trung tâm thông báo</span>
                {notifications.length > 0 && (
                  <div className="flex gap-3 text-[10px] font-bold text-primary">
                    <button onClick={handleMarkNotificationsAllRead} className="hover:underline cursor-pointer">Đã đọc hết</button>
                    <button onClick={handleClearNotifications} className="hover:underline text-slate-400 cursor-pointer">Xóa hết</button>
                  </div>
                )}
              </div>

              <div className="flex flex-col gap-3 max-h-[280px] overflow-y-auto">
                {notifications.length === 0 ? (
                  <p className="text-xs text-slate-400 text-center py-6">Không có thông báo mới nào!</p>
                ) : (
                  notifications.map((notif) => (
                    <div 
                      key={notif.id} 
                      className={`p-2 rounded-xl transition-all border ${
                        notif.read ? 'bg-white border-slate-50 text-slate-500' : 'bg-amber-50/40 border-amber-100/50 text-on-surface'
                      }`}
                    >
                      <div className="flex justify-between items-start">
                        <span className="text-xs font-bold leading-tight">{notif.title}</span>
                        <span className="text-[9px] text-slate-400 shrink-0">{notif.time}</span>
                      </div>
                      <p className="text-[10px] text-slate-500 mt-1 leading-normal">{notif.desc}</p>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

        {/* Identity selection switcher & current profile display */}
        <div className="relative pl-3 border-l border-slate-200">
          <button 
            onClick={() => {
              setShowUserDropdown(!showUserDropdown);
              setShowNotificationCenter(false);
              setShowMessagesCenter(false);
            }}
            className="flex items-center gap-2.5 hover:bg-slate-50 p-1.5 rounded-xl transition-all text-left cursor-pointer select-none"
          >
            <img 
              src={currentUser.avatar} 
              alt={currentUser.name} 
              className="w-9 h-9 rounded-xl object-cover border border-primary/20 shrink-0"
            />
            <div className="hidden lg:block">
              <div className="flex items-center gap-1">
                <p className="text-xs font-bold text-on-surface">{currentUser.name}</p>
                <ChevronDown className="w-3.5 h-3.5 text-slate-400" />
              </div>
              <p className="text-[10px] text-slate-400">{currentUser.role}</p>
            </div>
          </button>

          {/* Switch identity Dropdown */}
          {showUserDropdown && (
            <div className="absolute right-0 mt-2 w-64 bg-white rounded-2xl shadow-2xl border border-slate-100 py-2.5 z-50 animate-in fade-in slide-in-from-top-2 duration-150">
              <div className="px-4 py-2 border-b border-slate-100 mb-1 shrink-0">
                <span className="text-[10px] font-bold uppercase tracking-wider text-slate-400 block">Đổi vai trò đăng bài</span>
                <p className="text-[11px] text-slate-500 leading-snug">Chuyển sang tài khoản khác để trải nghiệm các quyền lợi khác nhau</p>
              </div>

              {usersList.map((user) => {
                const isSelected = currentUser.id === user.id;
                return (
                  <button
                    key={user.id}
                    onClick={() => {
                      onUserChange(user);
                      setShowUserDropdown(false);
                    }}
                    className={`w-full flex items-center justify-between px-4 py-2.5 text-left text-xs transition-colors cursor-pointer ${
                      isSelected ? 'bg-primary/5 text-primary font-bold' : 'hover:bg-slate-50 text-slate-600'
                    }`}
                  >
                    <div className="flex items-center gap-2.5 min-w-0">
                      <img src={user.avatar} alt={user.name} className="w-7 h-7 rounded-lg object-cover shrink-0" />
                      <div className="truncate">
                        <span className="font-bold text-slate-800 flex items-center gap-1.5">
                          {user.name}
                          {user.isAdmin && (
                            <span className="bg-red-100 text-red-700 font-extrabold text-[8px] px-1 rounded-sm leading-none uppercase">BQL</span>
                          )}
                        </span>
                        <p className="text-[10px] text-slate-400 truncate">{user.role}</p>
                      </div>
                    </div>
                    {isSelected && <Check className="w-4 h-4 text-primary shrink-0 ml-2 animate-bounce" />}
                  </button>
                );
              })}
            </div>
          )}
        </div>

      </div>
    </header>
  );
}
