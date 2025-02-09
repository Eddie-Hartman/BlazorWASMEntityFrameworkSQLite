let BWEFS = null;

export async function cacheInitialize(filename){
  if(!BWEFS){
    BWEFS = await caches.open('BlazorWASMEntityFrameworkSQLite');
  }

  const backupPath = `/${filename}`
  const cachePath = `/data/cache/${filename}`;

  const resp = await BWEFS.match(cachePath);

  if (resp?.ok) {
    const res = await resp.arrayBuffer();

    if (res) {
      console.log(`Restoring ${res.byteLength} bytes.`);
      Blazor.runtime.Module.FS.writeFile(backupPath, new Uint8Array(res));
      return true;
    }
  }
  return false;
}

export async function cacheSave(filename) {
  const backupPath = `/${filename}`;
  const cachePath = `/data/cache/${filename}`;

  if (Blazor.runtime.Module.FS.analyzePath(backupPath).exists) {
    const data = Blazor.runtime.Module.FS.readFile(backupPath);

    const blob = new Blob([data], {
      type: 'application/octet-stream'
    });

    const headers = new Headers({
      'content-length': blob.size
    });

    const response = new Response(blob, {
      headers
    });

    await BWEFS.put(cachePath, response);

    return true;
  }

  return false;
}

export async function cacheRestore(filename, arrayBuffer) {
  const backupPath = `/${filename}`;
  const cachePath = `/data/cache/${filename}`;

  if (arrayBuffer) {
    console.log(`Restoring ${arrayBuffer.byteLength} bytes.`);
    Blazor.runtime.Module.FS.writeFile(backupPath, new Uint8Array(arrayBuffer));

    const blob = new Blob([arrayBuffer], {
      type: 'application/octet-stream'
    });

    const headers = new Headers({
      'content-length': blob.size
    });

    const response = new Response(blob, {
      headers
    });

    await BWEFS.put(cachePath, response);

    return true;
  }

  return false;
}

export async function cacheGetDownloadURL(filename) {
  const cachePath = `/data/cache/${filename}`;
  const resp = await BWEFS.match(cachePath);

  if (resp?.ok) {
    const res = await resp.blob();
    if (res) {
      return URL.createObjectURL(res);
    }
  }

  return null;
}
