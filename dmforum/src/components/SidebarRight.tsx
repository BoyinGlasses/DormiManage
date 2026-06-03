import { useState } from 'react';
import { 
  CalendarDays, 
  Phone, 
  ShieldCheck, 
  Activity, 
  HelpCircle, 
  Building2, 
  Check, 
  Plus, 
  Smartphone,
  X
} from 'lucide-react';
import { StudentEvent, EmergencyContact } from '../types';

interface SidebarRightProps {
  events: StudentEvent[];
  onToggleEvent: (id: string) => void;
  contacts: EmergencyContact[];
}

export default function SidebarRight({ events, onToggleEvent, contacts }: SidebarRightProps) {
  const [selectedContact, setSelectedContact] = useState<EmergencyContact | null>(null);
  const [isCalling, setIsCalling] = useState(false);

  const handleContactClick = (contact: EmergencyContact) => {
    setSelectedContact(contact);
  };

  const startCall = () => {
    setIsCalling(true);
    setTimeout(() => {
      setIsCalling(false);
      setSelectedContact(null);
    }, 2800);
  };

  const getContactIcon = (iconName: string) => {
    switch (iconName) {
      case 'security':
        return <ShieldCheck className="w-5 h-5" />;
      case 'medical_services':
        return <Activity className="w-5 h-5 text-red-600" />;
      case 'admin_panel_settings':
        return <Building2 className="w-5 h-5 text-secondary" />;
      default:
        return <HelpCircle className="w-5 h-5" />;
    }
  };

  const getContactBg = (iconName: string) => {
    switch (iconName) {
      case 'security':
        return 'bg-blue-50 text-blue-600 border border-blue-100';
      case 'medical_services':
        return 'bg-red-50 text-red-600 border border-red-100';
      case 'admin_panel_settings':
        return 'bg-indigo-50 text-indigo-700 border border-indigo-100';
      default:
        return 'bg-slate-50 text-slate-500';
    }
  };

  return (
    <aside className="w-full lg:w-80 flex flex-col gap-6 py-2 sticky top-[72px] h-fit">
      
      {/* Student Activities Event Section */}
      <section className="bg-white rounded-xl p-5 card-shadow border border-slate-100">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-base font-bold font-display text-on-surface flex items-center gap-2">
            <CalendarDays className="w-5 h-5 text-primary" />
            Hoạt động SV
          </h3>
          <span className="text-xs text-slate-400 hover:text-primary cursor-pointer transition-colors font-medium">
            Tất cả
          </span>
        </div>

        <div className="flex flex-col gap-4">
          {events.map((event) => {
            const isJoined = event.joinedByMe;
            return (
              <div 
                key={event.id}
                id={`event-card-${event.id}`}
                onClick={() => onToggleEvent(event.id)}
                className="flex gap-3.5 group cursor-pointer p-1.5 rounded-lg hover:bg-slate-50 transition-all active:scale-[0.98]"
              >
                {/* Event Calendar Sheet Block */}
                <div className={`w-12 h-14 rounded-xl flex flex-col items-center justify-center border-b-[3px] transition-all shrink-0 ${
                  isJoined
                    ? 'bg-amber-500 text-white border-amber-600'
                    : 'bg-slate-100 text-on-surface border-slate-300 group-hover:bg-primary group-hover:text-white group-hover:border-primary-container'
                }`}>
                  <span className="text-[10px] uppercase font-bold tracking-wider leading-none">
                    {event.dateMonth}
                  </span>
                  <span className="text-lg font-extrabold leading-none mt-1">
                    {event.dateDay}
                  </span>
                </div>

                {/* Event details */}
                <div className="flex-1 min-w-0 flex flex-col justify-between">
                  <h4 className="text-xs font-bold leading-tight line-clamp-2 text-on-surface group-hover:text-primary transition-colors">
                    {event.title}
                  </h4>
                  <div className="flex justify-between items-end mt-1">
                    <span className="text-[11px] text-slate-400 truncate max-w-[130px]">
                      📍 {event.location}
                    </span>
                    <span className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${
                      isJoined 
                        ? 'bg-emerald-100 text-emerald-700' 
                        : 'bg-slate-100 text-slate-500'
                    }`}>
                      {isJoined ? 'Đã tham gia' : `+${event.participantsCount}`}
                    </span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </section>

      {/* Emergency Contacts Section */}
      <section className="bg-white rounded-xl p-5 card-shadow border border-slate-100">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-base font-bold font-display text-on-surface flex items-center gap-2">
            <Phone className="w-4 h-4 text-red-500" />
            Liên hệ khẩn cấp
          </h3>
          <span className="text-xs text-red-500 font-semibold uppercase tracking-wider animate-pulse">
            HOTLINE
          </span>
        </div>

        <div className="flex flex-col gap-3">
          {contacts.map((contact) => (
            <button
              onClick={() => handleContactClick(contact)}
              key={contact.id}
              id={`contact-${contact.id}`}
              className="w-full flex items-center gap-3.5 p-2 rounded-xl hover:bg-slate-50 border border-transparent hover:border-slate-100 text-left transition-all group cursor-pointer active:scale-[0.98]"
            >
              {/* Icon avatar of system agency */}
              <div className={`w-11 h-11 rounded-xl flex items-center justify-center shrink-0 ${getContactBg(contact.iconName)}`}>
                {getContactIcon(contact.iconName)}
              </div>

              {/* Text listing */}
              <div className="flex-1 min-w-0">
                <h4 className="text-xs font-bold text-on-surface group-hover:text-primary transition-colors truncate">
                  {contact.name}
                </h4>
                <p className="font-mono text-xs text-slate-400 font-medium">
                  {contact.phone}
                </p>
              </div>

              {/* Call indicator */}
              <span className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 group-hover:bg-primary/10 group-hover:text-primary transition-colors shrink-0">
                <Phone className="w-4 h-4" />
              </span>
            </button>
          ))}
        </div>
      </section>

      {/* Dialing Dialog Popup */}
      {selectedContact && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl max-w-sm w-full p-6 shadow-xl border border-slate-100 animate-in fade-in zoom-in duration-200">
            <div className="flex justify-between items-start mb-4">
              <span className="text-red-600 bg-red-50 px-3 py-1 rounded-full text-xs font-bold">
                ⚠️ Cuộc gọi khẩn cấp
              </span>
              <button 
                onClick={() => setSelectedContact(null)}
                className="text-slate-400 hover:text-slate-600 p-1 rounded-full hover:bg-slate-100 cursor-pointer"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="text-center py-4">
              <div className={`mx-auto w-16 h-16 rounded-full flex items-center justify-center mb-4 ${
                isCalling ? 'bg-emerald-500 text-white animate-bounce' : 'bg-slate-100 text-slate-600'
              }`}>
                {isCalling ? (
                  <Smartphone className="w-8 h-8 animate-pulse" />
                ) : (
                  <Phone className="w-8 h-8 text-primary" />
                )}
              </div>
              
              <h3 className="text-lg font-bold text-on-surface mb-1">
                {selectedContact.name}
              </h3>
              <p className="font-mono text-xl font-bold text-primary mb-3">
                {selectedContact.phone}
              </p>

              <p className="text-sm text-slate-500 leading-relaxed px-2">
                {isCalling 
                  ? "Hệ thống đang kết nối đường truyền ảo để thử nghiệm cuộc gọi điện..."
                  : "Màn hình mô phỏng: Bạn có chắc chắn muốn tiến hành cuộc gọi thực tế tới đường dây nóng này?"
                }
              </p>
            </div>

            <div className="flex gap-3 mt-4">
              <button
                onClick={() => setSelectedContact(null)}
                className="flex-1 py-2.5 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold text-sm transition-colors cursor-pointer"
                disabled={isCalling}
              >
                Hủy bỏ
              </button>
              <button
                onClick={startCall}
                className={`flex-1 py-2.5 rounded-lg text-white font-semibold text-sm transition-all cursor-pointer ${
                  isCalling 
                    ? 'bg-emerald-600 hover:bg-emerald-700 animate-pulse' 
                    : 'bg-primary hover:bg-primary-container'
                }`}
                disabled={isCalling}
              >
                {isCalling ? "Đang gọi..." : "Gọi ngay"}
              </button>
            </div>
          </div>
        </div>
      )}

    </aside>
  );
}
