// Types for Progress Tracking
export interface ProgressEntry {
  id: string;
  weight: number;
  bodyFatPercentage?: number;
  chest?: number;
  waist?: number;
  hips?: number;
  arms?: number;
  legs?: number;
  recordedAt: string;
  notes?: string;
}

export interface ProgressStatistics {
  period: string;
  totalEntries: number;
  startWeight: number;
  currentWeight: number;
  targetWeight?: number;
  totalWeightChange: number;
  averageWeightChange: number;
  averageWeight: number;
  trend: string; // Increasing, Decreasing, Stable
  weeksToGoal?: number;
  isOnTrack: boolean;
  lowestWeight: number;
  highestWeight: number;
  measurementChanges?: MeasurementChanges;
}

export interface MeasurementChanges {
  chest?: number;
  waist?: number;
  hips?: number;
  arms?: number;
  legs?: number;
}

export interface ProgressPhoto {
  id: string;
  photoUrl: string;
  photoType: string; // Front, Side, Back
  takenAt: string;
  weight?: number;
  notes?: string;
}

export interface CreateProgressRequest {
  weight: number;
  bodyFatPercentage?: number;
  chest?: number;
  waist?: number;
  hips?: number;
  arms?: number;
  legs?: number;
  recordedAt?: string;
  notes?: string;
}

// Legacy types for backward compatibility
export interface WeightLog extends ProgressEntry {
  loggedAt: string;
}
