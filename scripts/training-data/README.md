# Training Dataset Location

This folder is intentionally kept empty in git.

Store the real food training dataset outside the repository, then mount it into the API container via:

- `TRAINING_DATA_HOST_PATH` in `docker-compose.yml`
- Container path: `/app/scripts/training-data`

Expected structure inside the mounted dataset:

```text
training-data/
  pho_bo/
    001.jpg
  com_tam/
    001.jpg
  ...
```

Notes:

- One subfolder = one label/class.
- Keep only curated, correctly-labeled images.
- Do not commit full datasets into this repository.
