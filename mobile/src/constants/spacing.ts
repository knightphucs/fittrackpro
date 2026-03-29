// Spacing and sizing constants
export const spacing = {
  // Base spacing units
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 20,
  xxl: 24,
  xxxl: 32,

  // Screen padding
  screenHorizontal: 16,
  screenVertical: 20,

  // Component spacing
  cardPadding: 12,
  inputPadding: 16,
  buttonPadding: 16,
  listItemPadding: 12,
} as const;

export const borderRadius = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 20,
  full: 9999,
} as const;

export const iconSize = {
  xs: 12,
  sm: 16,
  md: 20,
  lg: 24,
  xl: 32,
  xxl: 48,
} as const;

export const hitSlop = {
  top: 10,
  bottom: 10,
  left: 10,
  right: 10,
} as const;
