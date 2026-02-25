export interface Account {
  id: string; 
  userId: string;
  accountName: string;
  accountNumber: string;
  iban: string;
  balance: number;
  availableBalance: number;
  currencyType: string; 
  providerBank: string;
  accountType: string;
  isActive: boolean;
  lastTransactionDate: string;
}