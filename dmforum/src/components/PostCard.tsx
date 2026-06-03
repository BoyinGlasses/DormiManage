import React, { useState } from 'react';
import { 
  Heart, 
  Eye, 
  MessageSquare, 
  Trash2, 
  MapPin, 
  Send, 
  AlertTriangle,
  Bookmark,
  Share2,
  Lock
} from 'lucide-react';
import { Post, Comment, User } from '../types';

interface PostCardProps {
  key?: string;
  post: Post;
  currentUser: User;
  onLikeToggle: (postId: string) => void;
  onDeletePost: (postId: string) => void;
  onAddComment: (postId: string, commentText: string) => void;
}

export default function PostCard({
  post,
  currentUser,
  onLikeToggle,
  onDeletePost,
  onAddComment,
}: PostCardProps) {
  const [commentInput, setCommentInput] = useState('');
  const [showComments, setShowComments] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  // Styling category color maps
  const getCategoryTheme = (category: string) => {
    switch (category) {
      case 'Tin tức chung':
        return 'bg-amber-100 text-amber-800';
      case 'CLB & Sự kiện':
        return 'bg-indigo-100 text-indigo-800';
      case 'Mua bán & Trao đổi':
        return 'bg-emerald-100 text-emerald-800';
      case 'Góp ý & Khiếu nại':
        return 'bg-rose-100 text-rose-800';
      default:
        return 'bg-slate-100 text-slate-800';
    }
  };

  const getAreaTagClass = (area: string) => {
    switch (area) {
      case 'Khu A':
        return 'text-indigo-700 bg-indigo-50 border-indigo-200';
      case 'Khu B':
        return 'text-primary bg-orange-50/50 border-primary-container/20';
      case 'Nhà ăn & Dịch vụ':
        return 'text-emerald-700 bg-emerald-50 border-emerald-200';
      default:
        return 'text-slate-600 bg-slate-50 border-slate-200';
    }
  };

  const handleCommentSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!commentInput.trim()) return;

    onAddComment(post.id, commentInput.trim());
    setCommentInput('');
  };

  const isMyPost = post.author.id === currentUser.id;

  return (
    <article 
      id={`post-card-${post.id}`}
      className="bg-white rounded-2xl overflow-hidden card-shadow hover:shadow-lg transition-all duration-300 border border-slate-100 flex flex-col md:flex-row mb-6 animate-in fade-in slide-in-from-bottom-4 duration-300"
    >
      {/* Featured Image Portion */}
      {post.imageUrl && (
        <div className="md:w-1/3 min-h-[220px] md:min-h-auto bg-slate-100 relative overflow-hidden group shrink-0">
          <div className="absolute inset-0 bg-gradient-to-br from-primary/15 to-secondary/15 mix-blend-multiply z-1" />
          <img 
            src={post.imageUrl} 
            alt={post.title} 
            className="w-full h-full object-cover select-none group-hover:scale-105 transition-all duration-700"
            loading="lazy"
          />

          {/* Area location card overlay (Glassmorphism as requested in the image) */}
          <div className="absolute bottom-4 left-4 right-4 bg-white/85 backdrop-blur-md p-3.5 rounded-xl border border-white/50 shadow-sm z-10 transition-all">
            <div className="flex justify-between items-end">
              <div>
                <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider leading-none">
                  Khu vực
                </p>
                <p className="text-sm font-extrabold text-primary font-display mt-0.5">
                  {post.area === 'Tất cả' ? 'Toàn KTX' : post.area}
                </p>
              </div>
              {post.isImportant && (
                <span className="text-red-600 text-xs font-black uppercase tracking-wider flex items-center gap-1 animate-pulse">
                  <AlertTriangle className="w-3 h-3 text-red-600" /> Quan trọng
                </span>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Structured Content portion */}
      <div className="flex-1 p-6 flex flex-col gap-4">
        
        {/* Category capsule and actions */}
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-2 flex-wrap">
            <span className={`text-[11px] font-bold px-3 py-1 rounded-full uppercase tracking-wider ${getCategoryTheme(post.category)}`}>
              {post.category}
            </span>
            {post.tags.map(tag => (
              <span key={tag} className="text-[11px] bg-slate-100 text-slate-600 px-2.5 py-0.5 rounded-full font-medium">
                {tag}
              </span>
            ))}
          </div>

          <div className="flex items-center gap-1.5 shrink-0">
            {isMyPost && (
              <button 
                onClick={() => onDeletePost(post.id)}
                title="Xóa bài viết của tôi"
                className="p-1 px-2 rounded-lg bg-red-50 hover:bg-red-100 text-red-600 hover:text-red-700 transition-colors text-xs font-bold cursor-pointer"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            )}
            <button 
              onClick={() => onLikeToggle(post.id)}
              className={`p-1.5 rounded-full hover:bg-slate-100 transition-colors cursor-pointer ${
                post.likedByMe ? 'text-primary' : 'text-slate-400 hover:text-primary'
              }`}
            >
              <Heart className={`w-5 h-5 transition-transform active:scale-130 ${post.likedByMe ? 'fill-primary text-primary' : ''}`} />
            </button>
          </div>
        </div>

        {/* Core title */}
        <div>
          <h2 className="text-xl font-bold font-display leading-snug text-on-surface hover:text-primary transition-colors cursor-pointer" onClick={() => setIsExpanded(!isExpanded)}>
            {post.title}
          </h2>
          <div className="mt-1 flex items-center gap-1.5 text-xs text-slate-400 font-medium">
            <MapPin className="w-3 h-3" />
            <span>Địa bàn: {post.area === 'Tất cả' ? 'Toàn bộ Ký túc xá' : `Khu vực ${post.area}`}</span>
          </div>
        </div>

        {/* Body content with toggle read-more */}
        <div>
          <p className={`text-sm text-on-surface-variant font-sans leading-relaxed ${isExpanded ? '' : 'line-clamp-3'}`}>
            {post.content}
          </p>
          {post.content.length > 200 && (
            <button 
              onClick={() => setIsExpanded(!isExpanded)}
              className="text-xs font-bold text-primary hover:underline mt-1.5 cursor-pointer block"
            >
              {isExpanded ? 'Thu gọn bớt' : 'Xem chi tiết bài viết'}
            </button>
          )}
        </div>

        {/* User Author Footer info & counters */}
        <div className="mt-auto pt-4 border-t border-slate-100 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          
          {/* Author Badge detail */}
          <div className="flex items-center gap-2.5">
            <img 
              src={post.author.avatar} 
              alt={post.author.name} 
              className="w-8 h-8 rounded-full object-cover border border-slate-200"
            />
            <div>
              <div className="flex items-center gap-1.5">
                <span className="text-xs font-bold text-on-surface">{post.author.name}</span>
                {post.author.isAdmin && (
                  <span className="bg-red-50 text-red-700 text-[9px] font-black px-1 rounded uppercase tracking-wider">
                    BQL
                  </span>
                )}
                {post.author.isClub && (
                  <span className="bg-secondary/10 text-secondary text-[9px] font-bold px-1 rounded">
                    CLB
                  </span>
                )}
              </div>
              <p className="text-[10px] text-slate-400 leading-none mt-0.5">
                {post.author.role} • {post.createdAt}
              </p>
            </div>
          </div>

          {/* Views Likes Messages interactions counts */}
          <div className="flex items-center gap-4 text-xs font-semibold text-slate-400">
            <span className="flex items-center gap-1 hover:text-slate-600 transition-colors p-1" title="Lượt xem bài viết">
              <Eye className="w-3.5 h-3.5" />
              <span>{post.views}</span>
            </span>
            <button 
              onClick={() => onLikeToggle(post.id)}
              className={`flex items-center gap-1 transition-colors p-1 rounded-md ${
                post.likedByMe ? 'text-primary bg-orange-50 px-1.5' : 'hover:text-primary'
              }`}
            >
              <Heart className={`w-3.5 h-3.5 ${post.likedByMe ? 'fill-primary text-primary' : ''}`} />
              <span>{post.likes}</span>
            </button>
            <button 
              onClick={() => setShowComments(!showComments)}
              className={`flex items-center gap-1 p-1 rounded-md transition-all ${
                showComments ? 'text-secondary bg-indigo-50 px-1.5' : 'hover:text-secondary'
              }`}
              title="Xem bình luận thảo luận"
            >
              <MessageSquare className="w-3.5 h-3.5" />
              <span>{post.comments.length}</span>
            </button>
          </div>

        </div>

        {/* Expandable active Comments Section */}
        {showComments && (
          <div className="mt-2 pt-4 border-t border-slate-50 bg-slate-50/50 -mx-6 -mb-6 p-6 transition-all duration-300">
            <h3 className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-3 flex items-center gap-1">
              💬 Thảo luận sinh viên ({post.comments.length})
            </h3>

            {/* List of comments */}
            <div className="flex flex-col gap-3.5 mb-4 max-h-[240px] overflow-y-auto pr-1">
              {post.comments.length === 0 ? (
                <p className="text-xs text-slate-400 text-center py-4 bg-white/60 rounded-xl border border-slate-100/50">
                  Chưa có bình luận nào tại đây. Hãy là người đầu tiên chia sẻ ý kiến của mình!
                </p>
              ) : (
                post.comments.map((comment) => (
                  <div key={comment.id} className="flex gap-2.5 items-start text-xs bg-white p-3 rounded-xl card-shadow border border-slate-100/30">
                    <img 
                      src={comment.authorAvatar} 
                      alt={comment.authorName} 
                      className="w-7 h-7 rounded-full object-cover border border-slate-100"
                    />
                    <div className="flex-1">
                      <div className="flex items-center justify-between mb-1">
                        <div className="flex items-center gap-1.5">
                          <span className="font-bold text-on-surface">{comment.authorName}</span>
                          <span className="text-[9px] text-slate-400 bg-slate-100 px-1 rounded-sm">
                            {comment.authorRole}
                          </span>
                        </div>
                        <span className="text-[10px] text-slate-400">{comment.createdAt}</span>
                      </div>
                      <p className="text-slate-600 leading-relaxed font-sans">{comment.content}</p>
                    </div>
                  </div>
                ))
              )}
            </div>

            {/* Adding review comment field */}
            <form onSubmit={handleCommentSubmit} className="flex gap-2 shrink-0">
              <input
                type="text"
                value={commentInput}
                onChange={(e) => setCommentInput(e.target.value)}
                placeholder="Viết phản hồi hoặc đặt câu hỏi ý kiến sinh viên..."
                className="flex-1 bg-white text-xs border border-slate-200 focus:ring-1 focus:ring-primary/20 rounded-xl px-3 py-2.5 outline-none focus:border-primary transition-all"
              />
              <button
                type="submit"
                className="bg-primary hover:bg-primary-container text-white p-2.5 rounded-xl transition-all cursor-pointer flex items-center justify-center shrink-0 shadow-sm"
              >
                <Send className="w-4 h-4" />
              </button>
            </form>
          </div>
        )}

      </div>
    </article>
  );
}
