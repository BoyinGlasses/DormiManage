import { User, Post, StudentEvent, EmergencyContact } from './types';

export const USERS: User[] = [
  {
    id: 'user-student-a',
    name: 'Nguyễn Văn A',
    role: 'Phòng 402 - Khu B',
    avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?q=80&w=256&auto=format&fit=crop',
    location: 'Khu B',
  },
  {
    id: 'user-bqm',
    name: 'Ban Quản Lý',
    role: 'Văn phòng KTX',
    avatar: 'https://images.unsplash.com/photo-1560250097-0b93528c311a?q=80&w=256&auto=format&fit=crop',
    location: 'Tất cả',
    isAdmin: true,
  },
  {
    id: 'user-volunteer',
    name: 'Đội Tình Nguyện',
    role: 'CLB Sinh Viên KTX',
    avatar: 'https://images.unsplash.com/photo-1582213782179-e0d53f98f2ca?q=80&w=256&auto=format&fit=crop',
    location: 'Tất cả',
    isClub: true,
  },
  {
    id: 'user-lananh',
    name: 'Lan Anh',
    role: 'Phòng 201 - Căng tin C',
    avatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?q=80&w=256&auto=format&fit=crop',
    location: 'Nhà ăn & Dịch vụ',
  },
];

export const INITIAL_EVENTS: StudentEvent[] = [
  {
    id: 'event-1',
    title: 'Giải bóng đá sinh viên nam KTX',
    dateMonth: 'Th5',
    dateDay: '12',
    location: 'Sân cỏ nhân tạo A',
    participantsCount: 42,
    joinedByMe: false,
  },
  {
    id: 'event-2',
    title: 'Đêm nhạc Acoustic gây quỹ',
    dateMonth: 'Th5',
    dateDay: '15',
    location: 'Sân sinh hoạt chung',
    participantsCount: 128,
    joinedByMe: true,
  },
  {
    id: 'event-3',
    title: 'Ngày hội Hiến máu nhân đạo 2026',
    dateMonth: 'Th6',
    dateDay: '02',
    location: 'Sảnh tòa nhà C',
    participantsCount: 15,
    joinedByMe: false,
  },
];

export const EMERGENCY_CONTACTS: EmergencyContact[] = [
  {
    id: 'contact-guard',
    name: 'Phòng Bảo vệ KTX',
    phone: '024 38xxx 111',
    iconName: 'security',
    colorClass: 'text-primary'
  },
  {
    id: 'contact-medical',
    name: 'Trạm y tế Tòa nhà C',
    phone: '024 38xxx 115',
    iconName: 'medical_services',
    colorClass: 'text-red-600'
  },
  {
    id: 'contact-bql',
    name: 'Văn phòng Ban Quản lý',
    phone: '024 38xxx 222',
    iconName: 'admin_panel_settings',
    colorClass: 'text-secondary'
  },
];

export const INITIAL_POSTS: Post[] = [
  {
    id: 'post-1',
    title: 'Thông báo lịch bảo trì điện nước khu B',
    content: 'Ban quản lý KTX xin thông báo: Từ 8h00 đến 11h30 sáng thứ 7 tuần này, khu B sẽ tạm ngưng cấp điện, nước để tiến hành bảo trì hệ thống định kỳ. Mong các bạn sinh viên chủ động sắp xếp sinh hoạt cá nhân, tích trữ nước dự phòng và tắt các thiết bị điện công suất lớn trước thời gian trên để tránh sự cố chập điện khi đóng điện trở lại.',
    category: 'Tin tức chung',
    tags: ['#thongbao', '#matdien'],
    area: 'Khu B',
    isImportant: true,
    imageUrl: 'https://images.unsplash.com/photo-1581092160607-ee22621dd758?q=80&w=800&auto=format&fit=crop',
    author: USERS[1], // Ban Quản Lý
    createdAt: '2 giờ trước',
    views: 1200,
    likes: 45,
    commentCount: 4,
    comments: [
      {
        id: 'comment-1-1',
        authorName: 'Nguyễn Văn A',
        authorAvatar: USERS[0].avatar,
        authorRole: 'Phòng 402 - Khu B',
        content: 'Cảm ơn Ban quản lý đã thông báo sớm ạ, để em biết đường đi tích trữ nước sạch.',
        createdAt: '1 giờ trước',
      },
      {
        id: 'comment-1-2',
        authorName: 'Trần Bình Minh',
        authorAvatar: 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?q=80&w=128&auto=format&fit=crop',
        authorRole: 'Phòng 512 - Khu B',
        content: 'Mất cả điện và nước luôn à ad ơi, hy vọng sửa đúng giờ chứ trưa nóng lắm.',
        createdAt: '45 phút trước',
      },
      {
        id: 'comment-1-3',
        authorName: 'Lê Thùy Chi',
        authorAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?q=80&w=128&auto=format&fit=crop',
        authorRole: 'Phòng 301 - Khu B',
        content: 'Ủng hộ bảo trì định kỳ ạ, dạo này nước vòi thi thoảng có màu hơi vàng.',
        createdAt: '30 phút trước',
      },
      {
        id: 'comment-1-4',
        authorName: 'Ban Quản Lý',
        authorAvatar: USERS[1].avatar,
        authorRole: 'Phòng Quản lý',
        content: 'Chào các bạn, Ban quản lý sẽ đôn đốc đội kỹ thuật thi công nhanh chóng và bàn giao lại hệ thống trước 11h30 nhé.',
        createdAt: '10 phút trước',
      }
    ],
  },
  {
    id: 'post-2',
    title: 'Tuyển thành viên CLB Tình nguyện KTX',
    content: 'CLB Tình nguyện đang tìm kiếm các bạn sinh viên nhiệt huyết tham gia chiến dịch "Mùa hè xanh" tại khuôn viên kí túc xá. Nếu bạn yêu thích các hoạt động cộng đồng, muốn đóng góp sức trẻ dọn dẹp thư viện, sơn sửa phòng tập thể thao, tổ chức đêm nhạc từ thiện và rèn luyện kỹ năng mềm cực chất, hãy nhanh tay đăng ký ứng tuyển ngay hôm nay nhé! Link form điền thông tin chi tiết được ghim trên fanpage.',
    category: 'CLB & Sự kiện',
    tags: ['#clbsukien', '#thethao'],
    area: 'Tất cả',
    isImportant: false,
    imageUrl: 'https://images.unsplash.com/photo-1559027615-cd4451a951e1?q=80&w=800&auto=format&fit=crop',
    author: USERS[2], // Đội Tình Nguyện
    createdAt: 'Hôm qua',
    views: 120,
    likes: 24,
    commentCount: 2,
    likedByMe: true,
    comments: [
      {
        id: 'comment-2-1',
        authorName: 'Nguyễn Văn A',
        authorAvatar: USERS[0].avatar,
        authorRole: 'Phòng 402 - Khu B',
        content: 'Form đăng ký còn mở đến bao giờ vậy ạ? Mình muốn tham gia cùng nhóm bạn thân.',
        createdAt: '20 giờ trước',
      },
      {
        id: 'comment-2-2',
        authorName: 'Đội Tình Nguyện',
        authorAvatar: USERS[2].avatar,
        authorRole: 'CLB Sinh Viên',
        content: 'Form mở đến hết ngày 30/5 bạn nhé! Chào đón bạn và nhóm tham gia nha.',
        createdAt: '18 giờ trước',
      }
    ],
  },
  {
    id: 'post-3',
    title: 'Review quán cơm mới mở cạnh căng tin khu C',
    content: 'Hôm nay mình mới thử quán cơm bình dân mới khai trương ở căng tin khu C. Suất 30k mà được khá nhiều thịt, có cả canh chua và trà đá miễn phí. Cô chủ quán rất nhiệt tình và vui vẻ, không gian ngồi máy lạnh mát mẻ sạch sẽ cực kỳ đề xuất cho mọi người đến ăn trưa nhé!',
    category: 'Mua bán & Trao đổi',
    tags: ['#cangtin', '#passdo'],
    area: 'Nhà ăn & Dịch vụ',
    isImportant: false,
    imageUrl: 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=800&auto=format&fit=crop',
    author: USERS[3], // Lan Anh
    createdAt: '3 ngày trước',
    views: 850,
    likes: 120,
    commentCount: 3,
    likedByMe: true,
    comments: [
      {
        id: 'comment-3-1',
        authorName: 'Hoàng Quốc Việt',
        authorAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?q=80&w=128&auto=format&fit=crop',
        authorRole: 'Phòng 104 - Khu A',
        content: 'Gần chỗ giữ xe đúng không bạn ơi? Trưa mai phải xách ví qua thử liền mới được.',
        createdAt: '2 ngày trước',
      },
      {
        id: 'comment-3-2',
        authorName: 'Lan Anh',
        authorAvatar: USERS[3].avatar,
        authorRole: 'Phòng 201',
        content: 'Đúng rồi á bạn, ngay sau cổng nhà xe đi bộ vào khoảng 15m bên tay trái nha.',
        createdAt: '2 ngày trước',
      },
      {
        id: 'comment-3-3',
        authorName: 'Phạm Minh Tuyết',
        authorAvatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?q=80&w=128&auto=format&fit=crop',
        authorRole: 'Phòng 415 - Khu B',
        content: 'Chỗ này bán cả tối nữa đó cậu, hôm qua tớ ăn suất đùi gà chiên mắm 35k ngon nhức nách!',
        createdAt: '1 ngày trước',
      }
    ],
  },
  {
    id: 'post-4',
    title: 'Góp ý về việc thu gom rác muộn tại hành lang Khu A',
    content: 'Chào ban quản lý, hiện tại nhân viên lao công thu gom rác hành lang tòa A thường dọn vào lúc 10-11h tối. Đôi khi sinh viên đi học về muộn túi rác để ngổn ngang gây bốc mùi khó chịu trong cả tối. Kính mong ban quản lý nghiên cứu đẩy giờ thu gom rác sang buổi chiều từ 4h - 6h hàng ngày để giữ vệ sinh chung tốt hơn.',
    category: 'Góp ý & Khiếu nại',
    tags: ['#thongbao'],
    area: 'Khu A',
    isImportant: false,
    imageUrl: 'https://images.unsplash.com/photo-1532996122724-e3c354a0b15b?q=80&w=800&auto=format&fit=crop',
    author: USERS[0], // Nguyễn Văn A
    createdAt: '4 ngày trước',
    views: 310,
    likes: 18,
    commentCount: 1,
    comments: [
      {
        id: 'comment-4-1',
        authorName: 'Ban Quản Lý',
        authorAvatar: USERS[1].avatar,
        authorRole: 'Ban Quản Lý KTX',
        content: 'Cảm ơn đóng góp ý kiến thiết thực của em. BQL đã ghi nhận và sẽ trao đổi lại với tổ vệ sinh để chấn chỉnh giờ giấc thu dọn rác khu A nhé.',
        createdAt: '3 ngày trước',
      }
    ],
  }
];
