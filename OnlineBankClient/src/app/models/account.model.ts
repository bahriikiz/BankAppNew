export interface Account {
  id: string;
  accountName: string;
  accountNumber: string;
  iban: string;
  balance: number;
  currency: string;
  providerBank: string | null; 
  userId: string;
}