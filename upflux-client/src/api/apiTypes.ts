export interface AdminLoginPayload {
    email: string;
    password: string;
}
  
export interface EngineerTokenPayload {
    engineerEmail: string;
    engineerName: string;
    machineIds: string[];
}
  

export interface LoginResponse {
    token?: string;
    error?: string;
}