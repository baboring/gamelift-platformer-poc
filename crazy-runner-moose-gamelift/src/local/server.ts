import {default as express} from 'express';
import { handler } from '../lambda/get-session';
const app = express();
app.get('/', async (_, response) => {
  try {
    const value = await handler();
    response.send(value);
  } catch (error) {
    console.error(error);
    response.sendStatus(500);
  }
});
app.listen(8081);
