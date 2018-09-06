import { FormControl } from '@angular/forms';

export class MockServerData
{
  path: string;
  passthroughOnFail: boolean;
  passthroughPath: PassthroughPath;
  routes: MockRoutes[];
  name: string;
  active: boolean;
}

export class MockRoutes
{
  headers: NamedParameter[];
  parameters: NamedParameter[];
  path: string;
  method: string;
  code: number;
  response: string;
}

export class PassthroughPath
{
  scheme: string;
  host: string;
  port: string;
}

export class NamedParameter
{
  key: string;
  value: string;
}
