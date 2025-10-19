from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from database.connection import get_db
from database import models
from schemas.user import UserCreate, UserOut
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from database.connection import get_db
from database import models
from schemas.user import UserCreate, UserOut, UserUpdate
from auth.passwords import hash_password, verify_password
from auth.security import require_admin, get_current_user
from typing import List, Optional

router = APIRouter()


# Public: create user (signup) - no auth required
@router.post('/', response_model=UserOut)
def create_user(payload: UserCreate, db: Session = Depends(get_db)):
    existing = db.query(models.User).filter((models.User.username == payload.username) | (models.User.email == payload.email)).first()
    if existing:
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail='Usuario o email ya existen')

    u = models.User(username=payload.username, full_name=payload.full_name, email=payload.email, hashed_password=hash_password(payload.password), role=models.UserRoleEnum(payload.role))
    db.add(u)
    db.commit()
    db.refresh(u)
    return u


# Protected: get own profile
@router.get('/me', response_model=UserOut)
def get_me(current_user: dict = Depends(get_current_user), db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.id == current_user.get('sub')).first()
    if not user:
        raise HTTPException(status_code=404, detail='Usuario no encontrado')
    return user


# Admin: list users
@router.get('/', response_model=List[UserOut])
def list_users(db: Session = Depends(get_db), current_user: dict = Depends(get_current_user)):
    if current_user.get('role') != 'admin':
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail='No tiene permisos')
    users = db.query(models.User).all()
    return users


# Get user by id (admin or owner)
@router.get('/{user_id}', response_model=UserOut)
def get_user(user_id: str, db: Session = Depends(get_db), current_user: dict = Depends(get_current_user)):
    user = db.query(models.User).filter(models.User.id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail='Usuario no encontrado')
    if current_user.get('role') != 'admin' and current_user.get('sub') != str(user.id):
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail='No tiene permisos')
    return user


# Update user (owner or admin). Non-admin cannot change role.
@router.put('/{user_id}', response_model=UserOut)
def update_user(user_id: str, payload: UserUpdate, db: Session = Depends(get_db), current_user: dict = Depends(get_current_user)):
    user = db.query(models.User).filter(models.User.id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail='Usuario no encontrado')

    is_admin = current_user.get('role') == 'admin'
    is_owner = current_user.get('sub') == str(user.id)
    if not (is_admin or is_owner):
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail='No tiene permisos')

    # non-admin cannot change role
    if payload.role and not is_admin:
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail='No puede cambiar el rol')

    if payload.full_name is not None:
        user.full_name = payload.full_name
    if payload.email is not None:
        user.email = payload.email
    if payload.password:
        user.hashed_password = hash_password(payload.password)
    if payload.role and is_admin:
        user.role = models.UserRoleEnum(payload.role)

    db.add(user)
    db.commit()
    db.refresh(user)
    return user


# Delete user (admin only)
@router.delete('/{user_id}')
def delete_user(user_id: str, db: Session = Depends(get_db), current_user: dict = Depends(get_current_user)):
    if current_user.get('role') != 'admin':
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail='No tiene permisos')
    user = db.query(models.User).filter(models.User.id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail='Usuario no encontrado')
    db.delete(user)
    db.commit()
    return {'message': 'Usuario eliminado'}