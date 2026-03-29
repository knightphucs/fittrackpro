// Date formatting utilities

/**
 * Format date to display string
 */
export const formatDate = (
  date: Date | string,
  locale: string = "vi-VN"
): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  return d.toLocaleDateString(locale, {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
};

/**
 * Format date to short string (DD/MM/YYYY)
 */
export const formatDateShort = (
  date: Date | string,
  locale: string = "vi-VN"
): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  return d.toLocaleDateString(locale);
};

/**
 * Format date to ISO string (YYYY-MM-DD)
 */
export const formatDateISO = (date: Date): string => {
  return date.toISOString().split("T")[0];
};

/**
 * Format time to display string
 */
export const formatTime = (
  date: Date | string,
  locale: string = "vi-VN"
): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  return d.toLocaleTimeString(locale, {
    hour: "2-digit",
    minute: "2-digit",
  });
};

/**
 * Format date and time
 */
export const formatDateTime = (
  date: Date | string,
  locale: string = "vi-VN"
): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  return d.toLocaleString(locale, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

/**
 * Get relative time string (e.g., "2 giờ trước")
 */
export const getRelativeTime = (date: Date | string): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return "Vừa xong";
  if (diffMins < 60) return `${diffMins} phút trước`;
  if (diffHours < 24) return `${diffHours} giờ trước`;
  if (diffDays < 7) return `${diffDays} ngày trước`;

  return formatDateShort(d);
};

/**
 * Check if date is today
 */
export const isToday = (date: Date | string): boolean => {
  const d = typeof date === "string" ? new Date(date) : date;
  const today = new Date();
  return (
    d.getDate() === today.getDate() &&
    d.getMonth() === today.getMonth() &&
    d.getFullYear() === today.getFullYear()
  );
};

/**
 * Get today's date as ISO string
 */
export const getTodayISO = (): string => {
  return formatDateISO(new Date());
};

/**
 * Get start of day
 */
export const startOfDay = (date: Date): Date => {
  const d = new Date(date);
  d.setHours(0, 0, 0, 0);
  return d;
};

/**
 * Get end of day
 */
export const endOfDay = (date: Date): Date => {
  const d = new Date(date);
  d.setHours(23, 59, 59, 999);
  return d;
};
