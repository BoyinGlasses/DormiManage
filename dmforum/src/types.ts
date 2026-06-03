export interface User {
  id: string;
  name: string;
  role: string;
  avatar: string;
  location: string;
  isAdmin?: boolean;
  isClub?: boolean;
}

export interface Comment {
  id: string;
  authorName: string;
  authorAvatar: string;
  authorRole: string;
  content: string;
  createdAt: string;
}

export interface Post {
  id: string;
  title: string;
  content: string;
  category: 'Tin tức chung' | 'CLB & Sự kiện' | 'Mua bán & Trao đổi' | 'Góp ý & Khiếu nại';
  tags: string[]; // e.g. ["#thongbao", "#matdien"]
  area: 'Khu A' | 'Khu B' | 'Nhà ăn & Dịch vụ' | 'Tất cả';
  imageUrl?: string;
  isImportant?: boolean;
  author: User;
  createdAt: string;
  views: number;
  likes: number;
  commentCount: number;
  comments: Comment[];
  likedByMe?: boolean;
}

export interface StudentEvent {
  id: string;
  title: string;
  dateMonth: string; // e.g. "Th5"
  dateDay: string; // e.g. "12"
  location: string;
  participantsCount: number;
  joinedByMe?: boolean;
}

export interface EmergencyContact {
  id: string;
  name: string;
  phone: string;
  iconName: 'security' | 'medical_services' | 'admin_panel_settings';
  colorClass: string; // text color or bg color
}
