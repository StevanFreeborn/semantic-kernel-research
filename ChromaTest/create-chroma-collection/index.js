import { ChromaClient } from 'chromadb';

const client = new ChromaClient({
  path: 'http://localhost:3111',
});

await client.deleteCollection({ name: "ChromaTest" });

let collection = await client.createCollection({
  name: "ChromaTest",
  metadata: { "hnsw:space": "cosine" },
});

console.log(collection);