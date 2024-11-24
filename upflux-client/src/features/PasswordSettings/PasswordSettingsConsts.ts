// PasswordSettingsConsts.ts

// Password validation constants
export const MIN_PASSWORD_LENGTH = 12;
export const HAS_NUMBER_REGEX = /\d/;
export const HAS_SPECIAL_CHAR_REGEX = /[!@#$%^&*(),.?":{}|<>]/;

// Error messages
export const PASSWORD_SET_NOT_ENOUGH_LENGTH = `Password must be at least ${MIN_PASSWORD_LENGTH} characters long.`;
export const PASSWORD_SET_NO_NUMBER = 'Password must contain at least one number.';
export const PASSWORD_SET_NO_SPECIAL_CHAR = 'Password must contain at least one special character.';
export const PASSWORD_CONFIRMATION_MISMATCH = 'Passwords do not match.';
