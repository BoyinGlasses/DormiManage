import React, { useState, useEffect } from 'react';
import Navbar from './components/Navbar';
import SidebarLeft from './components/SidebarLeft';
import SidebarRight from './components/SidebarRight';
import PostCard from './components/PostCard';
import CreatePostModal from './components/CreatePostModal';
import { Post, StudentEvent, User } from './types';
import { USERS, INITIAL_POSTS, INITIAL_EVENTS, EMERGENCY_CONTACTS } from './data';
import { PlusCircle, SearchX, Sparkles, Filter, Check, Trash2, CalendarDays } from 'lucide-react';

export default function App() {
  // Load state from localStorage or utilize initial presets
  const [currentUser, setCurrentUser] = useState<User>(() => {
    const saved = localStorage.getItem('dmforum_current_user');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        // Ensure we pointer link back to USERS array if needed
        const found = USERS.find(u => u.id === parsed.id);
        if (found) return found;
      } catch (err) {
        console.error("Local storage user parse error", err);
      }
    }
    return USERS[0]; // Nguyễn Văn A
  });

  const [postsList, setPostsList] = useState<Post[]>(() => {
    const saved = localStorage.getItem('dmforum_posts');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch (err) {
        console.error("Local storage posts parse error", err);
      }
    }
    return INITIAL_POSTS;
  });

  const [eventsList, setEventsList] = useState<StudentEvent[]>(() => {
    const saved = localStorage.getItem('dmforum_events');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch (err) {
        console.error("Local storage events parse error", err);
      }
    }
    return INITIAL_EVENTS;
  });

  // Filtering criteria parameters
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại' | null>(null);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const [selectedArea, setSelectedArea] = useState<'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ' | 'Tất cả' | null>(null);

  // Modal open controllers
  const [isCreatePostOpen, setIsCreatePostOpen] = useState(false);

  // Sync state upgrades persistent database triggers
  useEffect(() => {
    localStorage.setItem('dmforum_current_user', JSON.stringify(currentUser));
  }, [currentUser]);

  useEffect(() => {
    localStorage.setItem('dmforum_posts', JSON.stringify(postsList));
  }, [postsList]);

  useEffect(() => {
    localStorage.setItem('dmforum_events', JSON.stringify(eventsList));
  }, [eventsList]);

  // Compute live statistics for navigators counts
  const postCountsByFilter = postsList.reduce((acc, curr) => {
    // Increment category
    acc[curr.category] = (acc[curr.category] || 0) + 1;
    // Increment specific area
    if (curr.area && curr.area !== 'Tất cả') {
      acc[curr.area] = (acc[curr.area] || 0) + 1;
    }
    return acc;
  }, {} as { [key: string]: number });

  // Handle active user configuration shift
  const handleUserChange = (u: User) => {
    setCurrentUser(u);
  };

  // Interactions and counters triggers
  const handleLikeToggle = (postId: string) => {
    setPostsList(prev => prev.map(p => {
      if (p.id === postId) {
        const liked = !p.likedByMe;
        return {
          ...p,
          likedByMe: liked,
          likes: liked ? p.likes + 1 : Math.max(0, p.likes - 1)
        };
      }
      return p;
    }));
  };

  const handleDeletePost = (postId: string) => {
    if (confirm("Bạn có chắc chắn muốn xóa bài viết này khỏi diễn đàn?")) {
      setPostsList(prev => prev.filter(p => p.id !== postId));
    }
  };

  const handleAddComment = (postId: string, commentText: string) => {
    const newComment = {
      id: `comment-${Date.now()}`,
      authorName: currentUser.name,
      authorAvatar: currentUser.avatar,
      authorRole: currentUser.role,
      content: commentText,
      createdAt: 'Vừa xong'
    };

    setPostsList(prev => prev.map(p => {
      if (p.id === postId) {
        return {
          ...p,
          commentCount: p.commentCount + 1,
          comments: [...p.comments, newComment]
        };
      }
      return p;
    }));
  };

  const handlePostCreated = (newPost: Post) => {
    setPostsList([newPost, ...postsList]);
    setIsCreatePostOpen(false);
  };

  const handleToggleEvent = (eventId: string) => {
    setEventsList(prev => prev.map(ev => {
      if (ev.id === eventId) {
        const value = !ev.joinedByMe;
        return {
          ...ev,
          joinedByMe: value,
          participantsCount: value ? ev.participantsCount + 1 : Math.max(0, ev.participantsCount - 1)
        };
      }
      return ev;
    }));
  };

  const handleQuickNav = (category: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại' | 'All') => {
    if (category === 'All') {
      setSelectedCategory(null);
      setSelectedTag(null);
      setSelectedArea(null);
    } else {
      setSelectedCategory(category);
      setSelectedTag(null);
      setSelectedArea(null);
    }
  };

  // Filter Posts Logic
  const filteredPosts = postsList.filter(post => {
    // 1. Filter by category dropdown
    if (selectedCategory && post.category !== selectedCategory) {
      return false;
    }
    // 2. Filter by tag select
    if (selectedTag && !post.tags.includes(selectedTag)) {
      return false;
    }
    // 3. Filter by building region area
    if (selectedArea) {
      if (selectedArea === 'Tất cả') {
        // no-op, fits all
      } else if (post.area !== selectedArea) {
        return false;
      }
    }
    // 4. Case-insensitive text search string bar check
    if (searchQuery.trim()) {
      const normalizedQuery = searchQuery.toLowerCase();
      const matchTitle = post.title.toLowerCase().includes(normalizedQuery);
      const matchContent = post.content.toLowerCase().includes(normalizedQuery);
      const matchAuthor = post.author.name.toLowerCase().includes(normalizedQuery);
      const matchTags = post.tags.some(t => t.toLowerCase().includes(normalizedQuery));
      return matchTitle || matchContent || matchAuthor || matchTags;
    }

    return true;
  });

  return (
    <div className="bg-background min-h-screen pb-16">
      
      {/* Upper Navigation block */}
      <Navbar 
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        currentUser={currentUser}
        onUserChange={handleUserChange}
        usersList={USERS}
        onQuickNav={handleQuickNav}
      />

      {/* Primary body layout structure container */}
      <div className="max-w-7xl mx-auto px-4 sm:px-[32px] pt-24">
        
        {/* Horizontal filter summary tags indicators if matching filters */}
        {(selectedCategory || selectedTag || selectedArea || searchQuery) && (
          <div className="mb-4 bg-white/70 backdrop-blur-sm p-3.5 rounded-2xl border border-slate-100 flex items-center justify-between flex-wrap gap-2 animate-in fade-in duration-200">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-xs font-bold text-slate-400 flex items-center gap-1 uppercase tracking-wider scale-95 mr-1">
                <Filter className="w-3.5 h-3.5" /> Bộ lọc đang bật:
              </span>
              
              {selectedCategory && (
                <span className="inline-flex items-center gap-1 bg-amber-50 text-amber-800 border border-amber-200 text-xs px-2.5 py-1 rounded-full font-semibold">
                  Mục: {selectedCategory}
                  <button onClick={() => setSelectedCategory(null)} className="hover:text-red-600 font-bold ml-1 cursor-pointer">×</button>
                </span>
              )}

              {selectedTag && (
                <span className="inline-flex items-center gap-1 bg-slate-100 text-slate-700 border border-slate-200 text-xs px-2.5 py-1 rounded-full font-semibold">
                  Thẻ: {selectedTag}
                  <button onClick={() => setSelectedTag(null)} className="hover:text-red-600 font-bold ml-1 cursor-pointer">×</button>
                </span>
              )}

              {selectedArea && (
                <span className="inline-flex items-center gap-1 bg-indigo-50 text-indigo-800 border border-indigo-200 text-xs px-2.5 py-1 rounded-full font-semibold">
                  Khu: {selectedArea}
                  <button onClick={() => setSelectedArea(null)} className="hover:text-red-600 font-bold ml-1 cursor-pointer">×</button>
                </span>
              )}

              {searchQuery && (
                <span className="inline-flex items-center gap-1 bg-rose-50 text-rose-800 border border-rose-200 text-xs px-2.5 py-1 rounded-full font-semibold">
                  Từ khóa: "{searchQuery}"
                  <button onClick={() => setSearchQuery('')} className="hover:text-red-600 font-bold ml-1 cursor-pointer">×</button>
                </span>
              )}
            </div>

            <button 
              onClick={() => {
                setSelectedCategory(null);
                setSelectedTag(null);
                setSelectedArea(null);
                setSearchQuery('');
              }}
              className="text-xs font-bold text-primary hover:underline cursor-pointer"
            >
              Xóa tất cả bộ lọc
            </button>
          </div>
        )}

        {/* 3-column responsive system: SidebarLeft, Main Posts, SidebarRight */}
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          
          {/* Column Left: Side Navigation panels */}
          <div className="lg:col-span-1 hidden xl:block">
            <SidebarLeft 
              selectedCategory={selectedCategory}
              setSelectedCategory={setSelectedCategory}
              selectedTag={selectedTag}
              setSelectedTag={setSelectedTag}
              selectedArea={selectedArea}
              setSelectedArea={setSelectedArea}
              postCounts={postCountsByFilter}
            />
          </div>

          {/* Center Column: Interactive Posts feed */}
          <div className="col-span-1 lg:col-span-3 xl:col-span-2 flex flex-col gap-6">
            
            {/* Action Box to fast post (Matches screenshot block) */}
            <div className="bg-white p-4 rounded-2xl card-shadow flex items-center gap-4 border border-slate-100 shrink-0">
              <img 
                src={currentUser.avatar} 
                alt={currentUser.name} 
                className="w-10 h-10 rounded-full object-cover border border-slate-100 shrink-0 select-none"
              />
              <button
                onClick={() => setIsCreatePostOpen(true)}
                className="flex-1 bg-slate-50 text-slate-400 border border-transparent hover:border-slate-100 rounded-xl px-4 py-3 text-left text-sm transition-all cursor-pointer hover:bg-slate-100"
              >
                Bạn có thông tin gì muốn chia sẻ với mọi người, {currentUser.name}?
              </button>
              <button 
                onClick={() => setIsCreatePostOpen(true)}
                className="bg-primary-container hover:bg-primary text-white font-bold text-xs sm:text-sm px-4 py-2.5 rounded-xl transition-all cursor-pointer active:scale-95 shrink-0"
              >
                Đăng bài
              </button>
            </div>

            {/* Quick list responsive categories toggle buttons for smaller views */}
            <div className="xl:hidden flex items-center gap-2 overflow-x-auto pb-2 shrink-0">
              <button 
                onClick={() => handleQuickNav('All')}
                className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition-all whitespace-nowrap cursor-pointer ${
                  !selectedCategory ? 'bg-primary text-white' : 'bg-white text-slate-500 hover:bg-slate-50 border border-slate-100'
                }`}
              >
                Tất cả
              </button>
              <button 
                onClick={() => handleQuickNav('Tin tức chung')}
                className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition-all whitespace-nowrap cursor-pointer ${
                  selectedCategory === 'Tin tức chung' ? 'bg-primary text-white font-bold' : 'bg-white text-slate-500 hover:bg-slate-50 border border-slate-100'
                }`}
              >
                📰 Tin tức chung
              </button>
              <button 
                onClick={() => handleQuickNav('CLB & Sự kiện')}
                className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition-all whitespace-nowrap cursor-pointer ${
                  selectedCategory === 'CLB & Sự kiện' ? 'bg-primary text-white' : 'bg-white text-slate-500 hover:bg-slate-50 border border-slate-100'
                }`}
              >
                🎉 CLB & Sự kiện
              </button>
              <button 
                onClick={() => handleQuickNav('Mua bán & Trao đổi')}
                className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition-all whitespace-nowrap cursor-pointer ${
                  selectedCategory === 'Mua bán & Trao đổi' ? 'bg-primary text-white' : 'bg-white text-slate-500 hover:bg-slate-50 border border-slate-100'
                }`}
              >
                🛒 Mua bán & Trao đổi
              </button>
              <button 
                onClick={() => handleQuickNav('Góp ý & Khiếu nại')}
                className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition-all whitespace-nowrap cursor-pointer ${
                  selectedCategory === 'Góp ý & Khiếu nại' ? 'bg-primary text-white' : 'bg-white text-slate-500 hover:bg-slate-50 border border-slate-100'
                }`}
              >
                💬 Góp ý & Khiếu nại
              </button>
            </div>

            {/* Render any important posts that matching the area/category pinned to top */}
            {filteredPosts.some(p => p.isImportant) && (
              <div className="flex flex-col gap-1 shrink-0">
                <span className="text-[10px] uppercase font-extrabold tracking-wider text-red-600 flex items-center gap-1.5 px-1">
                  📌 Tin tức ghim quan trọng từ Ban Quản Lý
                </span>
                {filteredPosts.filter(p => p.isImportant).map((post) => (
                  <PostCard 
                    key={`pinned-${post.id}`}
                    post={post}
                    currentUser={currentUser}
                    onLikeToggle={handleLikeToggle}
                    onDeletePost={handleDeletePost}
                    onAddComment={handleAddComment}
                  />
                ))}
                <span className="text-[10px] uppercase font-extrabold tracking-wider text-slate-400 mt-4 border-t border-slate-100 pt-4 flex items-center gap-1.5 px-1">
                  📚 Tin đăng khác từ sinh viên
                </span>
              </div>
            )}

            {/* Feed List Render flow */}
            <div className="flex flex-col gap-1">
              {filteredPosts.filter(p => !p.isImportant).length === 0 ? (
                // Show Empty State if filters matching nothing of records
                <div className="bg-white text-center p-8 sm:p-12 rounded-2xl border border-slate-100 card-shadow flex flex-col items-center gap-4 animate-in fade-in duration-300">
                  <div className="w-16 h-16 rounded-full bg-slate-50 flex items-center justify-center text-slate-300">
                    <SearchX className="w-8 h-8" />
                  </div>
                  <div>
                    <h3 className="text-base font-bold font-display text-on-surface">Không tìm thấy bài viết nào</h3>
                    <p className="text-xs text-slate-400 max-w-sm mt-1">
                      Hãy thử giảm bớt các tiêu chí lọc hành lang/thẻ hoặc tìm kiếm bằng từ khóa ngắn gọn hơn xem sao nhé.
                    </p>
                  </div>
                  <button
                    onClick={() => {
                      setSelectedCategory(null);
                      setSelectedTag(null);
                      setSelectedArea(null);
                      setSearchQuery('');
                    }}
                    className="bg-primary hover:bg-primary-container text-white text-xs font-bold px-4 py-2.5 rounded-xl transition-all cursor-pointer"
                  >
                    Xem lại toàn bộ bài viết
                  </button>
                </div>
              ) : (
                filteredPosts.filter(p => !p.isImportant).map((post) => (
                  <PostCard 
                    key={post.id}
                    post={post}
                    currentUser={currentUser}
                    onLikeToggle={handleLikeToggle}
                    onDeletePost={handleDeletePost}
                    onAddComment={handleAddComment}
                  />
                ))
              )}
            </div>

          </div>

          {/* Right Column: Events & Crisis calls */}
          <div className="col-span-1 lg:col-span-3 xl:col-span-1">
            <SidebarRight 
              events={eventsList}
              onToggleEvent={handleToggleEvent}
              contacts={EMERGENCY_CONTACTS}
            />
          </div>

        </div>

      </div>

      {/* Popups and Modals Renderers */}
      {isCreatePostOpen && (
        <CreatePostModal 
          currentUser={currentUser}
          onClose={() => setIsCreatePostOpen(false)}
          onPostCreated={handlePostCreated}
        />
      )}

      {/* Footer support watermark */}
      <footer className="mt-16 text-center text-xs text-slate-400">
        <p>© 2026 DMForum - Hệ thống Diễn đàn Ký túc xá.</p>
        <p className="mt-1 font-mono text-[10px]">Phát triển bởi SV KTX Hub</p>
      </footer>

    </div>
  );
}
