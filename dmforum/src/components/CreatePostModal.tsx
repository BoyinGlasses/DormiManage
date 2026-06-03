import React, { useState } from 'react';
import { X, Image, Tag, MapPin, Layers, Sparkles, CheckCircle2 } from 'lucide-react';
import { User, Post } from '../types';

interface CreatePostModalProps {
  currentUser: User;
  onClose: () => void;
  onPostCreated: (post: Post) => void;
}

const PRESET_IMAGES = [
  {
    id: 'electronics',
    url: 'https://images.unsplash.com/photo-1581092160607-ee22621dd758?q=80&w=800&auto=format&fit=crop',
    label: 'Thiết bị & Kỹ thuật'
  },
  {
    id: 'discussion',
    url: 'https://images.unsplash.com/photo-1559027615-cd4451a951e1?q=80&w=800&auto=format&fit=crop',
    label: 'Đội nhóm & Chiến dịch'
  },
  {
    id: 'food',
    url: 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800&auto=format&fit=crop',
    label: 'Món ăn & Ẩm thực'
  },
  {
    id: 'green',
    url: 'https://images.unsplash.com/photo-1532996122724-e3c354a0b15b?q=80&w=800&auto=format&fit=crop',
    label: 'Môi trường & Dịch vụ'
  },
  {
    id: 'sports',
    url: 'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800&auto=format&fit=crop',
    label: 'Thể thao & Bóng đá'
  },
  {
    id: 'acoustic',
    url: 'https://images.unsplash.com/photo-1511192336575-5a79af67a629?q=80&w=800&auto=format&fit=crop',
    label: 'Âm nhạc & Sự kiện'
  }
];

export default function CreatePostModal({ currentUser, onClose, onPostCreated }: CreatePostModalProps) {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [category, setCategory] = useState<'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại'>('Tin tức chung');
  const [area, setArea] = useState<'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ' | 'Tất cả'>('Tất cả');
  const [selectedPresetImage, setSelectedPresetImage] = useState(PRESET_IMAGES[0].url);
  const [customImage, setCustomImage] = useState('');
  const [isImportant, setIsImportant] = useState(false);
  const [tagsInput, setTagsInput] = useState('');

  const [errors, setErrors] = useState<{ title?: string; content?: string }>({});

  const presetTags = ['#thongbao', '#clbsukien', '#passdo', '#cangtin', '#thethao', '#matdien'];
  const [chosenTags, setChosenTags] = useState<string[]>([]);

  const handlePresetTagToggle = (tag: string) => {
    if (chosenTags.includes(tag)) {
      setChosenTags(chosenTags.filter(t => t !== tag));
    } else {
      setChosenTags([...chosenTags, tag]);
    }
  };

  const handleFormSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const newErrors: { title?: string; content?: string } = {};
    if (!title.trim()) {
      newErrors.title = 'Vui lòng nhập tiêu đề bài đăng';
    }
    if (!content.trim()) {
      newErrors.content = 'Vui lòng viết nội dung bài đăng';
    }

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    // Process custom tags or preset tags
    let finalTags = [...chosenTags];
    if (tagsInput.trim()) {
      const parsed = tagsInput.split(',')
        .map(t => t.trim())
        .map(t => t.startsWith('#') ? t : `#${t}`)
        .filter(t => t.length > 1);
      finalTags = Array.from(new Set([...finalTags, ...parsed]));
    }

    // If no tags were chosen, default to simple tag corresponding to category
    if (finalTags.length === 0) {
      if (category === 'Tin tức chung') finalTags.push('#thongbao');
      else if (category === 'CLB & Sự kiện') finalTags.push('#clbsukien');
      else if (category === 'Mua bán & Trao đổi') finalTags.push('#passdo');
      else if (category === 'Góp ý & Khiếu nại') finalTags.push('#gopy');
    }

    const newPost: Post = {
      id: `post-user-${Date.now()}`,
      title: title.trim(),
      content: content.trim(),
      category,
      tags: finalTags,
      area,
      imageUrl: customImage.trim() || selectedPresetImage,
      isImportant: currentUser.isAdmin ? isImportant : false,
      author: currentUser,
      createdAt: 'Vừa xong',
      views: 1,
      likes: 0,
      commentCount: 0,
      comments: [],
    };

    onPostCreated(newPost);
  };

  return (
    <div className="fixed inset-0 bg-black/60 backdrop-blur-xs flex items-center justify-center z-50 p-4 overflow-y-auto">
      <div className="bg-white rounded-2xl max-w-2xl w-full my-8 shadow-2xl border border-slate-100 overflow-hidden animate-in fade-in zoom-in duration-200 flex flex-col max-h-[90vh]">
        
        {/* Header */}
        <div className="flex justify-between items-center px-6 py-4 border-b border-slate-100 shrink-0">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-orange-100 flex items-center justify-center text-primary">
              <Sparkles className="w-5 h-5 text-primary" />
            </div>
            <div>
              <h3 className="text-lg font-bold font-display text-on-surface">Đăng bài viết mới</h3>
              <p className="text-xs text-slate-400">Đăng tải lên bảng tin của cộng đồng KTX</p>
            </div>
          </div>
          <button 
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 p-1.5 rounded-full hover:bg-slate-100 cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content Flow */}
        <form onSubmit={handleFormSubmit} className="flex-1 overflow-y-auto p-6 flex flex-col gap-5">
          
          {/* User Badge Info */}
          <div className="flex items-center gap-3 bg-slate-50 p-3 rounded-xl border border-slate-100 shrink-0">
            <img 
              src={currentUser.avatar} 
              alt={currentUser.name} 
              className="w-10 h-10 rounded-full object-cover border-2 border-primary/20"
            />
            <div>
              <div className="flex items-center gap-1.5">
                <span className="text-sm font-bold text-on-surface">{currentUser.name}</span>
                {currentUser.isAdmin && (
                  <span className="bg-red-100 text-red-700 text-[10px] font-bold px-1.5 py-0.5 rounded">
                    Admin Ban Quản Lý
                  </span>
                )}
                {currentUser.isClub && (
                  <span className="bg-secondary/10 text-secondary text-[10px] font-bold px-1.5 py-0.5 rounded">
                    CLB SV
                  </span>
                )}
              </div>
              <p className="text-xs text-slate-400">{currentUser.role || 'Thành viên diễn đàn'}</p>
            </div>
          </div>

          {/* Form Title & Category Row */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
                Tiêu đề bài đăng *
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => {
                  setTitle(e.target.value);
                  if (e.target.value.trim() && errors.title) {
                    setErrors({ ...errors, title: undefined });
                  }
                }}
                placeholder="Nhập tiêu đề ngắn gọn súc tích..."
                className={`w-full bg-white text-sm border ${
                  errors.title ? 'border-red-500 focus:ring-red-500/20' : 'border-slate-200 focus:ring-primary/20'
                } rounded-lg px-3 py-2.5 outline-none focus:border-primary transition-all`}
              />
              {errors.title && <p className="text-xs text-red-500 mt-1">{errors.title}</p>}
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1 flex items-center gap-1">
                <Layers className="w-3 h-3 text-slate-400" /> Danh mục *
              </label>
              <select
                value={category}
                onChange={(e: any) => setCategory(e.target.value)}
                className="w-full bg-white text-sm border border-slate-200 rounded-lg px-3 py-2.5 outline-none focus:ring-primary/20 focus:border-primary transition-all"
              >
                <option value="Tin tức chung">📰 Tin tức chung</option>
                <option value="CLB & Sự kiện">🎉 CLB & Sự kiện</option>
                <option value="Mua bán & Trao đổi">🛒 Mua bán & Trao đổi</option>
                <option value="Góp ý & Khiếu nại">💬 Góp ý & Khiếu nại</option>
              </select>
            </div>
          </div>

          {/* Core Content Textarea */}
          <div>
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
              Nội dung chi tiết *
            </label>
            <textarea
              value={content}
              onChange={(e) => {
                setContent(e.target.value);
                if (e.target.value.trim() && errors.content) {
                  setErrors({ ...errors, content: undefined });
                }
              }}
              rows={4}
              placeholder="Bạn muốn chia sẻ điều gì với các bạn sinh viên? Hãy ghi chi tiết để mọi người dễ tương tác..."
              className={`w-full bg-white text-sm border ${
                errors.content ? 'border-red-500 focus:ring-red-500/20' : 'border-slate-200 focus:ring-primary/20'
              } rounded-lg px-3 py-2.5 outline-none focus:border-primary transition-all resize-none`}
            />
            {errors.content && <p className="text-xs text-red-500 mt-1">{errors.content}</p>}
          </div>

          {/* Area & Custom Tags Input Row */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1 flex items-center gap-1">
                <MapPin className="w-3.5 h-3.5 text-slate-400" /> Thuộc Khu vực
              </label>
              <select
                value={area}
                onChange={(e: any) => setArea(e.target.value)}
                className="w-full bg-white text-sm border border-slate-200 rounded-lg px-3 py-2.5 outline-none focus:ring-primary/20 focus:border-primary transition-all"
              >
                <option value="Tất cả">🌏 Tất cả các khu</option>
                <option value="Khu A">🏢 Tòa nhà Khu A</option>
                <option value="Khu B">🏬 Tòa nhà Khu B</option>
                <option value="Nhà ăn & Dịch vụ">🍽️ Nhà ăn & Dịch vụ (Khu C)</option>
              </select>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1 flex items-center gap-1">
                <Tag className="w-3.5 h-3.5 text-slate-400" /> Thẻ phụ tự viết
              </label>
              <input
                type="text"
                value={tagsInput}
                onChange={(e) => setTagsInput(e.target.value)}
                placeholder="ví dụ: #wifi, #passdo, #gopy (cách nhau dấu phẩy)"
                className="w-full bg-white text-sm border border-slate-200 rounded-lg px-3 py-2.5 outline-none focus:ring-primary/20 focus:border-primary transition-all"
              />
            </div>
          </div>

          {/* Preset Tags checklist */}
          <div>
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">
              Chọn nhanh Thẻ phổ biến
            </label>
            <div className="flex flex-wrap gap-1.5">
              {presetTags.map((tag) => {
                const checked = chosenTags.includes(tag);
                return (
                  <button
                    type="button"
                    key={tag}
                    onClick={() => handlePresetTagToggle(tag)}
                    className={`px-3 py-1 rounded-full text-xs font-medium cursor-pointer transition-all ${
                      checked 
                        ? 'bg-primary text-white font-semibold' 
                        : 'bg-slate-100 hover:bg-slate-200 text-slate-600'
                    }`}
                  >
                    {tag} {checked && '✓'}
                  </button>
                );
              })}
            </div>
          </div>

          {/* Visual Cover Chooser */}
          <div>
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2 flex items-center gap-1">
              <Image className="w-3.5 h-3.5 text-slate-400" /> Chọn ảnh bìa bài viết
            </label>
            
            <div className="grid grid-cols-3 sm:grid-cols-6 gap-2 mb-3">
              {PRESET_IMAGES.map((img) => {
                const isActive = selectedPresetImage === img.url && !customImage;
                return (
                  <button
                    type="button"
                    key={img.id}
                    onClick={() => {
                      setSelectedPresetImage(img.url);
                      setCustomImage('');
                    }}
                    className={`relative aspect-[4/3] rounded-lg overflow-hidden border-2 transition-all cursor-pointer ${
                      isActive ? 'border-primary shadow-md scale-102' : 'border-transparent hover:border-slate-300'
                    }`}
                  >
                    <img src={img.url} alt={img.label} className="w-full h-full object-cover" />
                    <div className="absolute inset-0 bg-black/25 flex items-end p-1">
                      <span className="text-[8px] text-white font-bold leading-tight truncate w-full uppercase">
                        {img.label}
                      </span>
                    </div>
                    {isActive && (
                      <div className="absolute top-1 right-1 bg-primary text-white rounded-full p-0.5">
                        <CheckCircle2 className="w-3 h-3" />
                      </div>
                    )}
                  </button>
                );
              })}
            </div>

            <input
              type="text"
              value={customImage}
              onChange={(e) => setCustomImage(e.target.value)}
              placeholder="Hoặc dán Link ảnh URL tùy chỉnh của bạn vào đây..."
              className="w-full bg-white text-xs border border-slate-200 rounded-lg px-3 py-2.5 outline-none focus:ring-primary/20 focus:border-primary transition-all"
            />
          </div>

          {/* Administrative special parameters */}
          {currentUser.isAdmin && (
            <div className="bg-red-50 hover:bg-red-50/80 p-3 rounded-xl border border-red-100 flex items-center justify-between shrink-0">
              <div>
                <span className="text-xs font-bold text-red-800">Thông báo từ Ban Quản Lý</span>
                <p className="text-[10px] text-red-600">Đánh dấu ghim bài viết này ở mục Tin tức quan trọng</p>
              </div>
              <label className="relative inline-flex items-center cursor-pointer">
                <input 
                  type="checkbox" 
                  checked={isImportant}
                  onChange={(e) => setIsImportant(e.target.checked)}
                  className="sr-only peer"
                />
                <div className="w-9 h-5 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:width-4 after:transition-all peer-checked:bg-red-600"></div>
              </label>
            </div>
          )}

        </form>

        {/* Footer actions */}
        <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex justify-end gap-3 shrink-0">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-semibold text-slate-500 hover:bg-slate-200 hover:text-slate-700 rounded-lg transition-colors cursor-pointer"
          >
            Đóng
          </button>
          <button
            type="button"
            onClick={handleFormSubmit}
            className="bg-primary hover:bg-primary-container text-white px-5 py-2 rounded-lg text-sm font-bold shadow-sm transition-all cursor-pointer hover:scale-101 active:scale-99"
          >
            Đăng bài ngay
          </button>
        </div>

      </div>
    </div>
  );
}
