// Types for Food
export interface Food {
  id: string;
  name: string;
  nameVi?: string;
  category?: string;
  servingSize: number;
  servingUnit: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  fiber?: number;
  sugar?: number;
  imageUrl?: string;
}

export interface FoodDetail extends Food {
  brand?: string;
  isUserCreated: boolean;
}

export interface FoodCategory {
  id: string;
  name: string;
  nameVi?: string;
  description?: string;
}
