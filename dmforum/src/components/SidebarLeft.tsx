import { 
  Newspaper, 
  Users, 
  Store, 
  MessageSquare, 
  Tag, 
  MapPin, 
  ChevronRight, 
  Compass 
} from 'lucide-react';

interface SidebarLeftProps {
  selectedCategory: string | null;
  setSelectedCategory: (cat: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại' | null) => void;
  selectedTag: string | null;
  setSelectedTag: (tag: string | null) => void;
  selectedArea: string | null;
  setSelectedArea: (area: 'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ' | 'Tất cả' | null) => void;
  postCounts: { [key: string]: number };
}

export default function SidebarLeft({
  selectedCategory,
  setSelectedCategory,
  selectedTag,
  setSelectedTag,
  selectedArea,
  setSelectedArea,
  postCounts,
}: SidebarLeftProps) {
  const categories: { name: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại'; icon: any; colorClass: string }[] = [
    { name: 'Tin tức chung', icon: Newspaper, colorClass: 'text-primary' },
    { name: 'CLB & Sự kiện', icon: Users, colorClass: 'text-secondary' },
    { name: 'Mua bán & Trao đổi', icon: Store, colorClass: 'text-tertiary' },
    { name: 'Góp ý & Khiếu nại', icon: MessageSquare, colorClass: 'text-orange-600' },
  ];

  const hashtags = ['#thongbao', '#clbsukien', '#passdo', '#cangtin', '#thethao', '#matdien'];

  const areas: { name: 'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ'; letter: string; color: string }[] = [
    { name: 'Khu A', letter: 'A', color: 'bg-indigo-100 text-indigo-700 hover:bg-indigo-600 hover:text-white' },
    { name: 'Khu B', letter: 'B', color: 'bg-primary/10 text-primary hover:bg-primary hover:text-white' },
    { name: 'Nhà ăn & Dịch vụ', letter: 'C', color: 'bg-emerald-100 text-emerald-700 hover:bg-emerald-600 hover:text-white' },
  ];

  const handleCategoryClick = (categoryName: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại') => {
    if (selectedCategory === categoryName) {
      setSelectedCategory(null); // toggle off
    } else {
      setSelectedCategory(categoryName);
      setSelectedTag(null); // clear tag filter to prioritize category
    }
  };

  const handleTagClick = (tag: string) => {
    if (selectedTag === tag) {
      setSelectedTag(null);
    } else {
      setSelectedTag(tag);
    }
  };

  const handleAreaClick = (area: 'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ') => {
    if (selectedArea === area) {
      setSelectedArea(null);
    } else {
      setSelectedArea(area);
    }
  };

  const clearFilters = () => {
    setSelectedCategory(null);
    setSelectedTag(null);
    setSelectedArea(null);
  };

  const hasAnyFilter = selectedCategory || selectedTag || selectedArea;

  return (
    <aside className="w-full xl:w-64 flex flex-col gap-6 py-2 sticky top-[72px] h-fit">
      
      {/* Category Section */}
      <section className="flex flex-col gap-2">
        <div className="flex justify-between items-center px-2">
          <h3 className="text-xs font-semibold text-on-surface-variant/70 uppercase tracking-wider font-label">
            Danh mục
          </h3>
          {hasAnyFilter && (
            <button 
              onClick={clearFilters}
              className="text-[11px] font-medium text-primary hover:underline cursor-pointer"
            >
              Đặt lại
            </button>
          )}
        </div>
        <nav className="flex flex-col gap-1">
          {categories.map((cat) => {
            const Icon = cat.icon;
            const isSelected = selectedCategory === cat.name;
            const count = postCounts[cat.name] || 0;

            return (
              <button
                key={cat.name}
                id={`cat-${cat.name.replace(/\s+/g, '-').toLowerCase()}`}
                onClick={() => handleCategoryClick(cat.name)}
                className={`w-full flex items-center justify-between px-3 py-2.5 rounded-lg transition-all text-left group cursor-pointer ${
                  isSelected 
                    ? 'text-primary font-bold bg-primary/10 border-l-4 border-primary' 
                    : 'text-on-surface-variant hover:bg-surface-container-low'
                }`}
              >
                <div className="flex items-center gap-3">
                  <Icon className={`w-5 h-5 ${isSelected ? 'text-primary' : 'text-slate-400 group-hover:text-primary transition-colors'}`} />
                  <span className="text-sm font-medium">{cat.name}</span>
                </div>
                {count > 0 && (
                  <span className={`text-xs px-2 py-0.5 rounded-full ${isSelected ? 'bg-primary text-white' : 'bg-slate-100 text-slate-500'}`}>
                    {count}
                  </span>
                )}
              </button>
            );
          })}
        </nav>
      </section>

      {/* Popular Tags Section */}
      <section className="flex flex-col gap-2">
        <h3 className="px-2 text-xs font-semibold text-on-surface-variant/70 uppercase tracking-wider font-label flex items-center gap-1.5">
          <Tag className="w-3.5 h-3.5 text-slate-400" />
          Thẻ phổ biến
        </h3>
        <div className="flex flex-wrap gap-1.5 px-2">
          {hashtags.map((tag) => {
            const isSelected = selectedTag === tag;
            return (
              <button
                key={tag}
                id={`tag-${tag.replace('#', '')}`}
                onClick={() => handleTagClick(tag)}
                className={`px-3 py-1 rounded-full text-xs font-medium transition-all active:scale-95 cursor-pointer ${
                  isSelected
                    ? 'bg-primary text-white shadow-sm'
                    : 'bg-white text-on-surface-variant/80 hover:bg-primary/10 hover:text-primary border border-slate-100'
                }`}
              >
                {tag}
              </button>
            );
          })}
        </div>
      </section>

      {/* Area/Location Section */}
      <section className="flex flex-col gap-2">
        <div className="flex justify-between items-center px-2">
          <h3 className="text-xs font-semibold text-on-surface-variant/70 uppercase tracking-wider font-label flex items-center gap-1.5">
            <MapPin className="w-3.5 h-3.5 text-slate-400" />
            Khu vực
          </h3>
          <ChevronRight className="w-4 h-4 text-slate-400 cursor-pointer hover:text-primary" />
        </div>
        <nav className="flex flex-col gap-1.5">
          {areas.map((area) => {
            const isSelected = selectedArea === area.name;
            const count = postCounts[area.name] || 0;

            return (
              <button
                key={area.name}
                id={`area-${area.name.replace(/\s+/g, '-').toLowerCase()}`}
                onClick={() => handleAreaClick(area.name)}
                className={`w-full flex items-center justify-between p-2 rounded-lg transition-all text-left group cursor-pointer ${
                  isSelected 
                    ? 'bg-primary/5 text-primary border border-primary/20 shadow-sm' 
                    : 'text-on-surface-variant hover:bg-surface-container-low'
                }`}
              >
                <div className="flex items-center gap-3">
                  <span className={`w-8 h-8 rounded-lg flex items-center justify-center font-bold text-sm transition-all ${
                    isSelected ? 'bg-primary text-white' : area.color
                  }`}>
                    {area.letter}
                  </span>
                  <div>
                    <span className="text-sm font-medium">{area.name}</span>
                    <p className="text-[10px] text-slate-400 font-normal">
                      Ký túc xá
                    </p>
                  </div>
                </div>
                {count > 0 && (
                  <span className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${isSelected ? 'bg-primary text-white' : 'bg-slate-100 text-slate-500'}`}>
                    {count} bài
                  </span>
                )}
              </button>
            );
          })}

          <button
            onClick={() => setSelectedArea(selectedArea === 'Tất cả' ? null : 'Tất cả')}
            className={`w-full flex items-center gap-3 p-2 rounded-lg transition-all text-left group cursor-pointer ${
              selectedArea === 'Tất cả'
                ? 'bg-primary/5 text-primary border border-primary/20 shadow-sm'
                : 'text-on-surface-variant hover:bg-surface-container-low'
            }`}
          >
            <span className={`w-8 h-8 rounded-lg flex items-center justify-center font-bold text-sm transition-all ${
              selectedArea === 'Tất cả' ? 'bg-primary text-white' : 'bg-slate-100 text-slate-500 group-hover:bg-primary/10 group-hover:text-primary'
            }`}>
              <Compass className="w-4 h-4" />
            </span>
            <div>
              <span className="text-sm font-medium">Tất cả các khu</span>
              <p className="text-[10px] text-slate-400 font-normal">Toàn bộ khuôn viên</p>
            </div>
          </button>
        </nav>
      </section>
      
    </aside>
  );
}
